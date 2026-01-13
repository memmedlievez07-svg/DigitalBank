
using DigitalBank.Domain.Entities.Identity;

namespace DigitalBank.Application.Interfaces
{
    public interface IJwtTokenService
    {
        Task<(string Token, DateTime ExpiresAtUtc)> CreateAccessTokenAsync(AppUser user);
    }
}
