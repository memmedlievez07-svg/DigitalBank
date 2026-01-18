using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Client
{
    [Authorize]
    [Route("api/client/notifications")]
    public class NotificationsController : ApiControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> My([FromQuery] NotificationFilterDto filter)
            => FromResult(await _service.GetMyAsync(filter));

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
            => FromResult(await _service.GetUnreadCountAsync());

        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkRead([FromRoute] int id)
            => FromResult(await _service.MarkReadAsync(id));
    }
}
