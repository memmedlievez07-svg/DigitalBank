using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/dashboard")]
    public class DashboardController : ApiControllerBase
    {
        private readonly IAdminDashboardService _service;

        public DashboardController(IAdminDashboardService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
            => FromResult(await _service.GetAsync());
    }
}
