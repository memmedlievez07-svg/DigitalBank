using DigitalBank.Application.Dtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Options;
using DigitalBank.Application.Results;
using DigitalBank.Domain.Entities;
using DigitalBank.Domain.Entities.Identity;
using DigitalBank.Domain.Enums;
using DigitalBank.Persistence.Dal;
using DigitalBank.Persistence.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace DigitalBank.Persistence.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly DigitalBankDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAuditLogService _audit;
        private readonly IEmailSender _emailSender;

        private readonly JwtOption _jwt;
        private readonly IConfiguration _config;

        public AuthService(
            DigitalBankDbContext db,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IJwtTokenService jwtTokenService,
            IAuditLogService audit,
            IEmailSender emailSender,
            IOptions<JwtOption> jwtOptions,
            IConfiguration config)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _audit = audit;
            _emailSender = emailSender;
            _jwt = jwtOptions.Value;
            _config = config;
        }

        public async Task<ServiceResultVoid> RegisterAsync(RegisterRequestDto dto)
        {
            // 1) Check email exists
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
            {
                await _audit.WriteAsync(AuditActionType.Register, false, "Email already exists",
                    detailsJson: $"{{\"email\":\"{dto.Email}\"}}");
                return ServiceResultVoid.Fail("Email artıq mövcuddur", 400);
            }

            // 2) Create user
            var user = new AppUser
            {
                Email = dto.Email,
                UserName = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                FatherName = dto.FatherName,
                Address = dto.Address,
                Age = dto.Age,
                EmailConfirmed = false
            };

            var createRes = await _userManager.CreateAsync(user, dto.Password);
            if (!createRes.Succeeded)
            {
                var errs = createRes.Errors.Select(e => e.Description).ToList();
                await _audit.WriteAsync(AuditActionType.Register, false, "Register failed",
                    detailsJson: $"{{\"email\":\"{dto.Email}\",\"errors\":\"{string.Join(" | ", errs)}\"}}");
                return ServiceResultVoid.Fail(errs, "Register failed", 400);
            }

            // 3) Assign default role
            // (Rol seed olunubsa problemsizdir)
            await _userManager.AddToRoleAsync(user, "User");

            // 4) Generate confirmation token + link
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);

            var apiBaseUrl = _config["ApiBaseUrl"]?.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                // API base yoxdursa da register keçsin, sadəcə email göndərməyək
                await _audit.WriteAsync(AuditActionType.Register, true, "Register success but ApiBaseUrl missing",
                    detailsJson: $"{{\"email\":\"{dto.Email}\"}}",
                    overrideUserId: user.Id);

                return ServiceResultVoid.Ok("Qeydiyyat uğurludur. Email təsdiqi üçün link konfiqurasiya olunmayıb.");
            }

            var confirmLink = $"{apiBaseUrl}/api/client/auth/confirm-email?userId={user.Id}&token={encodedToken}";

            // 5) Send email
            var subject = "DigitalBank — Email təsdiqi";
            var html = EmailTemplates.ConfirmEmail(
    displayName: user.FirstName ?? user.UserName ?? "Hörmətli müştəri",
    confirmLink: confirmLink);

            await _emailSender.SendAsync(user.Email!, subject, html);

            // 6) Audit
            await _audit.WriteAsync(AuditActionType.Register, true, "Register success",
                detailsJson: $"{{\"email\":\"{dto.Email}\"}}",
                overrideUserId: user.Id);

            return ServiceResultVoid.Ok("Qeydiyyat uğurludur. Email-ə təsdiq linki göndərildi.");
        }

        public async Task<ServiceResultVoid> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResultVoid.Fail("User tapılmadı", 404);

            var res = await _userManager.ConfirmEmailAsync(user, token);
            if (!res.Succeeded)
            {
                var errs = res.Errors.Select(e => e.Description).ToList();
                return ServiceResultVoid.Fail(errs, "Email confirm failed", 400);
            }

            return ServiceResultVoid.Ok("Email təsdiqləndi.");
        }

        public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null)
            {
                await _audit.WriteAsync(AuditActionType.Login, false, "User not found",
                    detailsJson: $"{{\"email\":\"{dto.Email}\"}}");
                return ServiceResult<AuthResponseDto>.Fail("Email və ya şifrə yanlışdır", 400);
            }

            if (!user.EmailConfirmed)
            {
                await _audit.WriteAsync(AuditActionType.Login, false, "Email not confirmed",
                    detailsJson: $"{{\"email\":\"{dto.Email}\"}}",
                    overrideUserId: user.Id);

                return ServiceResult<AuthResponseDto>.Fail("Email təsdiqlənməyib", 400);
            }

            // LockoutOnFailure=true → yanlış cəhdlərdə lockout işləyir
            var signIn = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!signIn.Succeeded)
            {
                await _audit.WriteAsync(AuditActionType.Login, false, "Invalid credentials",
                    detailsJson: $"{{\"email\":\"{dto.Email}\"}}",
                    overrideUserId: user.Id);

                return ServiceResult<AuthResponseDto>.Fail("Email və ya şifrə yanlışdır", 400);
            }

            var (accessToken, accessExp) = await _jwtTokenService.CreateAccessTokenAsync(user);

            // refresh token create + save
            var refreshRaw = GenerateRefreshTokenRaw();
            var refreshExp = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);

            var rt = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshRaw,             // Sadə versiya: raw saxlayırıq
                CreatedDate = DateTime.UtcNow,
                ExpiresAt = refreshExp
            };

            _db.RefreshTokens.Add(rt);
            await _db.SaveChangesAsync();

            await _audit.WriteAsync(AuditActionType.Login, true, "Login success",
                detailsJson: $"{{\"email\":\"{dto.Email}\"}}",
                overrideUserId: user.Id);

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAtUtc = accessExp,

                RefreshToken = refreshRaw,
                RefreshTokenExpiresAtUtc = refreshExp,

                User = new UserBriefDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? user.Email ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };

            return ServiceResult<AuthResponseDto>.Ok(response);
        }

        public async Task<ServiceResult<AuthResponseDto>> RefreshAsync(RefreshTokenRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            {
                await _audit.WriteAsync(AuditActionType.Refresh, false, "Refresh token empty");
                return ServiceResult<AuthResponseDto>.Fail("Refresh token boşdur", 400);
            }

            var rt = await _db.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.TokenHash == dto.RefreshToken);

            if (rt == null)
            {
                await _audit.WriteAsync(AuditActionType.Refresh, false, "Refresh token not found");
                return ServiceResult<AuthResponseDto>.Fail("Refresh token yanlışdır", 400);
            }

            if (rt.RevokedAt != null)
            {
                await _audit.WriteAsync(AuditActionType.Refresh, false, "Refresh token revoked",
                    overrideUserId: rt.UserId);
                return ServiceResult<AuthResponseDto>.Fail("Refresh token etibarsızdır", 400);
            }

            if (rt.ExpiresAt <= DateTime.UtcNow)
            {
                await _audit.WriteAsync(AuditActionType.Refresh, false, "Refresh token expired",
                    overrideUserId: rt.UserId);
                return ServiceResult<AuthResponseDto>.Fail("Refresh token müddəti bitib", 400);
            }

            var user = rt.User!;
            var (accessToken, accessExp) = await _jwtTokenService.CreateAccessTokenAsync(user);

            // Rotation: revoke old + create new
            var newRefreshRaw = GenerateRefreshTokenRaw();
            var newRefreshExp = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);

            rt.RevokedAt = DateTime.UtcNow;
            rt.ReplacedByTokenHash = newRefreshRaw;

            var newRt = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newRefreshRaw,
                CreatedDate = DateTime.UtcNow,
                ExpiresAt = newRefreshExp
            };

            _db.RefreshTokens.Add(newRt);
            await _db.SaveChangesAsync();

            await _audit.WriteAsync(AuditActionType.Refresh, true, "Refresh success",
                overrideUserId: user.Id);

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAtUtc = accessExp,

                RefreshToken = newRefreshRaw,
                RefreshTokenExpiresAtUtc = newRefreshExp,

                User = new UserBriefDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? user.Email ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };

            return ServiceResult<AuthResponseDto>.Ok(response);
        }

        public async Task<ServiceResultVoid> LogoutAsync(RefreshTokenRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            {
                await _audit.WriteAsync(AuditActionType.Logout, false, "Refresh token empty");
                return ServiceResultVoid.Fail("Refresh token boşdur", 400);
            }

            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == dto.RefreshToken);
            if (rt == null)
            {
                await _audit.WriteAsync(AuditActionType.Logout, false, "Refresh token not found");
                return ServiceResultVoid.Fail("Refresh token tapılmadı", 404);
            }

            if (rt.RevokedAt == null)
            {
                rt.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            await _audit.WriteAsync(AuditActionType.Logout, true, "Logout",
                overrideUserId: rt.UserId);

            return ServiceResultVoid.Ok("Çıxış edildi.");
        }

        private static string GenerateRefreshTokenRaw()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    // ====== Email templates helper (keçid: aşağıda ayrıca tam kodunu da verirəm) ======

}
