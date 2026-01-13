using DigitalBank.Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Base
{
    public class ApiControllerBase : ControllerBase
    {
        protected IActionResult FromResult(ServiceResultVoid result)
             => StatusCode(result.StatusCode, result);

        protected IActionResult FromResult<T>(ServiceResult<T> result)
            => StatusCode(result.StatusCode, result);
    }
}
