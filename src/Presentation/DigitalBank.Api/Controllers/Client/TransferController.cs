using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Dtos;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Client
{
    [Authorize]
    [Route("api/client/transfer")]
    public class TransferController : ApiControllerBase
    {
        private readonly ITransferService _service;

        public TransferController(ITransferService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
            => FromResult(await _service.TransferAsync(dto));
    }
}
