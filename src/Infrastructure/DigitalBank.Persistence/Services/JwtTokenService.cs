using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Options;
using DigitalBank.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DigitalBank.Persistence.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOption _jwt;
        private readonly UserManager<AppUser> _userManager;

        public JwtTokenService(IOptions<JwtOption> jwtOptions, UserManager<AppUser> userManager)
        {
            _jwt = jwtOptions.Value;
            _userManager = userManager;
        }

        public async Task<(string Token, DateTime ExpiresAtUtc)> CreateAccessTokenAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? "")
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));//rbac

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

            var jwt = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return (token, expires);
        }
    }
}
