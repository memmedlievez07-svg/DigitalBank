//using DigitalBank.Api.Controllers.Base;
//using DigitalBank.Application.Dtos.AdminDashBoardDtos;
//using DigitalBank.Application.Interfaces;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace DigitalBank.Api.Controllers.Admin
//{
//    [Authorize(Roles = "Admin")]
//    [Route("api/admin/notifications")]
//    public class AdminNotificationsController : ApiControllerBase
//    {
//        private readonly IAdminNotificationService _service;

//        public AdminNotificationsController(IAdminNotificationService service)
//        {
//            _service = service;
//        }

//        [HttpPost("broadcast")]
//        public async Task<IActionResult> Broadcast([FromBody] AdminSendNotificationDto dto)
//            => FromResult(await _service.SendToAllAsync(dto));

//        [HttpPost("to-user")]
//        public async Task<IActionResult> ToUser([FromBody] AdminSendToUserDto dto)
//            => FromResult(await _service.SendToUserAsync(dto));
//    }
//}
