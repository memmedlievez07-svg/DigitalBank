using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Dtos.Message;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Client
{
    [Authorize]
    [Route("api/client/chat")]
    public class ChatController : ApiControllerBase
    {
        private readonly IChatService _service;

        public ChatController(IChatService service)
        {
            _service = service;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
            => FromResult(await _service.SendAsync(dto));

        [HttpGet("history")]
        public async Task<IActionResult> History([FromQuery] ChatHistoryFilterDto filter)
            => FromResult(await _service.GetHistoryAsync(filter));

        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkRead([FromRoute] int id)
            => FromResult(await _service.MarkReadAsync(id));
    }
}
