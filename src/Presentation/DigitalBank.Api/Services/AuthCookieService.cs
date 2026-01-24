using DigitalBank.Application.Dtos;

namespace DigitalBank.Api.Services
{
    public class AuthCookieService : IAuthCookieService
    {
        private const string CookieName = "refreshToken";

        public void SetRefreshTokenCookie(HttpResponse response, AuthResponseDto dto)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                // HTTPS (localhost:7055) istifadə etdiyin üçün Secure mütləq true olmalıdır
                Secure = true,
                // Fərqli portlar (5173 -> 7055) üçün None mütləqdir
                SameSite = SameSiteMode.None,
                Expires = dto.RefreshTokenExpiresAtUtc,
                Path = "/"
            };

            response.Cookies.Append(CookieName, dto.RefreshToken, options);
        }

        public string? ReadRefreshToken(HttpRequest request)
            => request.Cookies.TryGetValue(CookieName, out var token) ? token : null;

        public void DeleteRefreshTokenCookie(HttpResponse response)
        {
            response.Cookies.Delete(CookieName, new CookieOptions
            {
                Path = "/"
            });
        }
    }
}
