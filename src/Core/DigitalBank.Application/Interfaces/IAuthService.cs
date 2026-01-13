using DigitalBank.Application.Dtos;
using DigitalBank.Application.Results;
using Microsoft.AspNetCore.Identity.Data;

namespace DigitalBank.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResultVoid> RegisterAsync(RegisterRequestDto request);
        Task<ServiceResultVoid> ConfirmEmailAsync(string userId, string token);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ServiceResult<AuthResponseDto>> RefreshAsync(RefreshTokenRequestDto request);
        Task<ServiceResultVoid> LogoutAsync(RefreshTokenRequestDto dto);
    }
}
