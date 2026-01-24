using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Dtos.AdminDashBoardDtos.Wallet;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/wallets")]
    public class AdminWalletsController : ApiControllerBase
    {
        private readonly IAdminWalletService _service;

        public AdminWalletsController(IAdminWalletService service)
        {
            _service = service;
        }

        // Wallet-ləri siyahılamaq (Filtr və Pagination ilə)
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] AdminWalletFilterDto filter)
            => FromResult(await _service.ListAsync(filter));

        // Wallet statusunu yeniləmək (Bloklamaq və ya Aktiv etmək)
        [HttpPatch("{walletId}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] int walletId, [FromBody] UpdateWalletStatusDto dto)
            => FromResult(await _service.UpdateStatusAsync(walletId, dto));
    }
}