namespace DigitalBank.Api.Middlewares
{
    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-Id";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            // client göndəribsə götür, yoxdursa yarat
            var cid = context.Request.Headers[HeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(cid))
                cid = Guid.NewGuid().ToString("N");

            // request scope-da saxla
            context.Items[HeaderName] = cid;

            // response-da da qaytar (debug üçün superdir)
            context.Response.Headers[HeaderName] = cid;

            await _next(context);
        }
    }
}
