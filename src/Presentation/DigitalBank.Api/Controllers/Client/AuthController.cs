using DigitalBank.Api.Controllers.Base;
using DigitalBank.Api.Services;
using DigitalBank.Application.Dtos;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Client
{
    [Route("api/client/auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IAuthCookieService _cookies;

        public AuthController(IAuthService auth, IAuthCookieService cookies)
        {
            _auth = auth;
            _cookies = cookies;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
            => FromResult(await _auth.RegisterAsync(dto));

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
            => FromResult(await _auth.ConfirmEmailAsync(userId, token));

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _auth.LoginAsync(dto);
            if (result.Success && result.Data != null)
            {
                _cookies.SetRefreshTokenCookie(Response, result.Data);
                result.Data.RefreshToken = "***"; // body-də göstərməyək
            }
            return FromResult(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            var token = string.IsNullOrWhiteSpace(dto.RefreshToken)
                ? _cookies.ReadRefreshToken(Request)
                : dto.RefreshToken;

            var result = await _auth.RefreshAsync(new RefreshTokenRequestDto { RefreshToken = token ?? "" });

            if (result.Success && result.Data != null)
            {
                _cookies.SetRefreshTokenCookie(Response, result.Data);
                result.Data.RefreshToken = "***";
            }

            return FromResult(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
        {
            var token = string.IsNullOrWhiteSpace(dto.RefreshToken)
                ? _cookies.ReadRefreshToken(Request)
                : dto.RefreshToken;

            var result = await _auth.LogoutAsync(new RefreshTokenRequestDto { RefreshToken = token ?? "" });

            _cookies.DeleteRefreshTokenCookie(Response);
            return FromResult(result);
        }
    }
}
