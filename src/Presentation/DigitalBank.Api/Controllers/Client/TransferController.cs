using Microsoft.AspNetCore.Mvc;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Dtos;

namespace DigitalBank.WebAPI.Controllers
{
    [ApiController]
    // Frontend '/api/client/transfer' gözləyir:
    [Route("api/client/[controller]")]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;

        public TransferController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        // Frontend '/send' axtarır:
        [HttpPost("send")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
        {
            var result = await _transferService.TransferAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // Frontend '/recent' axtarır:
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent()
        {
            var result = await _transferService.GetRecentTransfersAsync();
            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var result = await _transferService.GetTransactionHistoryAsync();
            return Ok(result);
        }
    }
}