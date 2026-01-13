using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/transactions")]
    public class AdminTransactionsController : ApiControllerBase
    {
        private readonly IAdminTransactionService _service;

        public AdminTransactionsController(IAdminTransactionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] AdminTransactionFilterDto filter)
            => FromResult(await _service.ListAsync(filter));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
            => FromResult(await _service.GetByIdAsync(id));
    }
}
