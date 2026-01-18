using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Client
{
    [Authorize]
    [Route("api/client/wallet")]
    public class WalletController : ApiControllerBase
    {
        private readonly IWalletService _service;

        public WalletController(IWalletService service)
        {
            _service = service;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
            => FromResult(await _service.GetMyWalletAsync());
    }
}
