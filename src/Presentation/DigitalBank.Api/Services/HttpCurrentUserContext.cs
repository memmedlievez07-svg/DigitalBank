using DigitalBank.Api.Middlewares;
using DigitalBank.Application.Interfaces;
using System.Security.Claims;

namespace DigitalBank.Api.Services
{
    public class HttpCurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _accessor;

        public HttpCurrentUserContext(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string? UserId
            => _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string? IpAddress
            => _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string? UserAgent
            => _accessor.HttpContext?.Request?.Headers.UserAgent.ToString();

        public string CorrelationId
        {
            get
            {
                var ctx = _accessor.HttpContext;
                if (ctx == null) return Guid.NewGuid().ToString("N");

                if (ctx.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var v) && v is string s && !string.IsNullOrWhiteSpace(s))
                    return s;

                // fallback (çox nadir)
                return ctx.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            }
        }
    }
}
