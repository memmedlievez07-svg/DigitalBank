using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Dtos;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/users")]
    public class AdminUsersController : ApiControllerBase
    {
        private readonly IAdminUserService _service;

        public AdminUsersController(IAdminUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
            => FromResult(await _service.GetUsersAsync());

        [HttpPatch("{userId}/lock")]
        public async Task<IActionResult> Lock([FromRoute] string userId)
            => FromResult(await _service.LockUserAsync(userId));

        [HttpPatch("{userId}/unlock")]
        public async Task<IActionResult> Unlock([FromRoute] string userId)
            => FromResult(await _service.UnlockUserAsync(userId));

        [HttpPut("{userId}/role")]
        public async Task<IActionResult> SetRole([FromRoute] string userId, [FromBody] SetUserRoleRequestDto dto)
            => FromResult(await _service.SetRoleAsync(userId, dto));
    }
}
