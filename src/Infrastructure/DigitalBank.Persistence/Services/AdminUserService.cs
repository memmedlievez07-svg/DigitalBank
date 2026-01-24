using DigitalBank.Application.Dtos;
using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Domain.Entities.Identity;
using DigitalBank.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditLogService _audit;

        public AdminUserService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuditLogService audit)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _audit = audit;
        }

        public async Task<ServiceResult<List<AdminUserListItemDto>>> GetUsersAsync()
        {
            // 1. Birbaşa Select atırıq. Bu zaman EF Core arxa planda avtomatik Join edir.
            // Include yazmağa ehtiyac qalmır və EF asılılığı yaranmır.
            var userQuery = _userManager.Users.Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                Email = u.Email ?? "",
                UserName = u.UserName ?? "",
                EmailConfirmed = u.EmailConfirmed,
                LockoutEndUtc = u.LockoutEnd,
                IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow,

                // Wallet üzərindən balansı çəkirik. Select daxilində olduğu üçün EF bunu başa düşür.
                Balance = u.Wallet != null ? u.Wallet.Balance : 0,

                // Qeyd: GetRolesAsync-i burada birbaşa Select daxilində istifadə edə bilməzsən (SQL-ə çevrilmir)
            });

            var list = userQuery.ToList();

            // 2. Rolları yaddaşa yüklədikdən sonra foreach ilə əlavə edirik
            foreach (var dto in list)
            {
                var user = _userManager.Users.First(x => x.Id == dto.Id);
                var roles = await _userManager.GetRolesAsync(user);
                dto.Roles = roles.ToList();
            }

            return ServiceResult<List<AdminUserListItemDto>>.Ok(list);
        }

        public async Task<ServiceResultVoid> LockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await _audit.WriteAsync(AuditActionType.UserLock, false, "User tapılmadı",
                    detailsJson: $"{{\"targetUserId\":\"{userId}\"}}");
                return ServiceResultVoid.Fail("User tapılmadı", 404);
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
            {
                var msg = string.Join("; ", res.Errors.Select(e => e.Description));
                await _audit.WriteAsync(AuditActionType.UserLock, false, msg,
                    detailsJson: $"{{\"targetUserId\":\"{userId}\"}}");
                return ServiceResultVoid.Fail(res.Errors.Select(e => e.Description).ToList(), "Lock failed", 400);
            }

            await _audit.WriteAsync(AuditActionType.UserLock, true, "User locked",
                detailsJson: $"{{\"targetUserId\":\"{userId}\"}}");

            return ServiceResultVoid.Ok("User bloklandı");
        }

        public async Task<ServiceResultVoid> UnlockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await _audit.WriteAsync(AuditActionType.UserUnlock, false, "User tapılmadı",
                    detailsJson: $"{{\"targetUserId\":\"{userId}\"}}");
                return ServiceResultVoid.Fail("User tapılmadı", 404);
            }

            user.LockoutEnd = null;

            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
            {
                var msg = string.Join("; ", res.Errors.Select(e => e.Description));
                await _audit.WriteAsync(AuditActionType.UserUnlock, false, msg,
                    detailsJson: $"{{\"targetUserId\":\"{userId}\"}}");
                return ServiceResultVoid.Fail(res.Errors.Select(e => e.Description).ToList(), "Unlock failed", 400);
            }

            await _audit.WriteAsync(AuditActionType.UserUnlock, true, "User unlocked",
                detailsJson: $"{{\"targetUserId\":\"{userId}\"}}");

            return ServiceResultVoid.Ok("User açıldı");
        }

        public async Task<ServiceResultVoid> SetRoleAsync(string userId, SetUserRoleRequestDto dto)
        {
            var role = dto.Role?.Trim();

            if (string.IsNullOrWhiteSpace(role))
            {
                await _audit.WriteAsync(AuditActionType.UserSetRole, false, "Role boş ola bilməz",
                    detailsJson: $"{{\"targetUserId\":\"{userId}\",\"role\":null}}");
                return ServiceResultVoid.Fail("Role boş ola bilməz", 400);
            }

            if (role != "Admin" && role != "User")
            {
                await _audit.WriteAsync(AuditActionType.UserSetRole, false, "Role yalnız Admin/User ola bilər",
                    detailsJson: $"{{\"targetUserId\":\"{userId}\",\"role\":\"{role}\"}}");
                return ServiceResultVoid.Fail("Role yalnız Admin/User ola bilər", 400);
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _audit.WriteAsync(AuditActionType.UserSetRole, false, "Role sistemdə mövcud deyil",
                    detailsJson: $"{{\"targetUserId\":\"{userId}\",\"role\":\"{role}\"}}");
                return ServiceResultVoid.Fail("Role sistemdə mövcud deyil", 400);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await _audit.WriteAsync(AuditActionType.UserSetRole, false, "User tapılmadı",
                    detailsJson: $"{{\"targetUserId\":\"{userId}\",\"role\":\"{role}\"}}");
                return ServiceResultVoid.Fail("User tapılmadı", 404);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var remove = currentRoles.Where(r => r == "Admin" || r == "User").ToList();

            if (remove.Any())
            {
                var remRes = await _userManager.RemoveFromRolesAsync(user, remove);
                if (!remRes.Succeeded)
                {
                    var msg = string.Join("; ", remRes.Errors.Select(e => e.Description));
                    await _audit.WriteAsync(AuditActionType.UserSetRole, false, msg,
                        detailsJson: $"{{\"targetUserId\":\"{userId}\",\"role\":\"{role}\"}}");
                    return ServiceResultVoid.Fail(remRes.Errors.Select(e => e.Description).ToList(), "Role remove failed", 400);
                }
            }

            var addRes = await _userManager.AddToRoleAsync(user, role);
            if (!addRes.Succeeded)
            {
                var msg = string.Join("; ", addRes.Errors.Select(e => e.Description));
                await _audit.WriteAsync(AuditActionType.UserSetRole, false, msg,
                    detailsJson: $"{{\"targetUserId\":\"{userId}\",\"role\":\"{role}\"}}");
                return ServiceResultVoid.Fail(addRes.Errors.Select(e => e.Description).ToList(), "Role add failed", 400);
            }

            await _audit.WriteAsync(AuditActionType.UserSetRole, true, "Role changed",
                detailsJson: $"{{\"targetUserId\":\"{userId}\",\"role\":\"{role}\"}}");

            return ServiceResultVoid.Ok("Rol dəyişdirildi");
        }

        public async Task<ServiceResult<List<UserLookUpDto>>> SearchUsersByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length < 3)
                return ServiceResult<List<UserLookUpDto>>.Ok(new List<UserLookUpDto>());

            var users = await _userManager.Users
                .AsNoTracking()
                .Where(u => u.Email.Contains(email))
                .Take(10)
                .Select(u => new UserLookUpDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToListAsync();

            return ServiceResult<List<UserLookUpDto>>.Ok(users);
        }
    }
}
