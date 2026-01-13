using DigitalBank.Application.Dtos;

namespace DigitalBank.Api.Services
{
    public interface IAuthCookieService
    {
        void SetRefreshTokenCookie(HttpResponse response, AuthResponseDto dto);
        string? ReadRefreshToken(HttpRequest request);
        void DeleteRefreshTokenCookie(HttpResponse response);
    }
}
