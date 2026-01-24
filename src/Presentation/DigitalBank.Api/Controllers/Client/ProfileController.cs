using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Dtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Persistence.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Client
{
    [Authorize]
    [Route("api/client/profile")]
    public class ProfileController : ApiControllerBase
    {
        private readonly IProfileService _service;

        public ProfileController(IProfileService service)
        {
            _service = service;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            return FromResult(await _service.GetMyProfileAsync());
        }

        [HttpPut("me")]
        public async Task<IActionResult> Update([FromBody] ProfileUpdateRequestDto dto)
            => FromResult(await _service.UpdateMyProfileAsync(dto));

        [HttpPost("avatar")]
        [RequestSizeLimit(2 * 1024 * 1024)]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file, CancellationToken ct)
            => FromResult(await _service.UploadAvatarAsync(file, ct));

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var result = await _service.SearchUsersAsync(query);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
