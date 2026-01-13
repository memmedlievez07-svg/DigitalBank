using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/audit-logs")]
    public class AuditLogsController : ApiControllerBase
    {
        private readonly IAuditLogService _service;

        public AuditLogsController(IAuditLogService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] AuditLogFilterDto filter)
            => FromResult(await _service.SearchAsync(filter));
    }
}
