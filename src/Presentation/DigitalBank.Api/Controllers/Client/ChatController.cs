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

        // Tək bir mesajı oxundu etmək üçün (Mövcud idi)
        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkReadAsync([FromRoute] int id)
            => FromResult(await _service.MarkReadAsync(id));

        // --- YENİ ƏLAVƏ EDİLMƏLİ OLAN METOD ---
        // Bütün söhbəti (peerUserId ilə olan) oxundu etmək üçün
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead([FromQuery] string peerUserId)
        {
            var result = await _service.MarkAllReadAsync(peerUserId);
            return FromResult(result);
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var result = await _service.GetMyConversationsAsync();
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}