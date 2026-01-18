using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/notifications")]
    public class AdminSendNotificationsController : ApiControllerBase
    {
        private readonly IAdminNotificationService _service;

        public AdminSendNotificationsController(IAdminNotificationService service)
        {
            _service = service;
        }

        // Hamıya göndər
        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] AdminSendNotificationDto dto)
            => FromResult(await _service.SendToAllAsync(dto));

        // Tək user-ə göndər
        [HttpPost("to-user")]
        public async Task<IActionResult> ToUser([FromBody] AdminSendToUserDto dto)
            => FromResult(await _service.SendToUserAsync(dto));
    }
}
