using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Dtos;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/users")]
    public class UsersController : ApiControllerBase
    {
        private readonly IAdminUserService _users;

        public UsersController(IAdminUserService users)
        {
            _users = users;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
            => FromResult(await _users.GetUsersAsync());

        [HttpPost("{id}/lock")]
        public async Task<IActionResult> Lock(string id)
            => FromResult(await _users.LockUserAsync(id));

        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> Unlock(string id)
            => FromResult(await _users.UnlockUserAsync(id));

        [HttpPost("{id}/set-role")]
        public async Task<IActionResult> SetRole(string id, [FromBody] SetUserRoleRequestDto dto)
            => FromResult(await _users.SetRoleAsync(id, dto));
    }
}
