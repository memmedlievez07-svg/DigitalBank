using DigitalBank.Api.Middlewares;
using DigitalBank.Application.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Security.Claims;
using System.Text.Json;

namespace DigitalBank.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // Client request-i yarımçıq kəsibsə (tab bağladı və s.)
                return;
            }
            catch (Exception ex)
            {
                // CorrelationId (ErrorId kimi istifadə edirik)
                var errorId = context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var v)
                    ? v?.ToString()
                    : context.TraceIdentifier;

                // UserId log üçün
                var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

                // Log (developer burdan tapacaq)
                Log.Error(ex,
                    "Unhandled exception | ErrorId={ErrorId} | UserId={UserId} | {Method} {Path}",
                    errorId,
                    userId,
                    context.Request.Method,
                    context.Request.Path);

                // Status code map
                var statusCode = ex switch
                {
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    ArgumentException => StatusCodes.Status400BadRequest,
                    InvalidOperationException => StatusCodes.Status400BadRequest,
                    BadHttpRequestException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };

                if (context.Response.HasStarted)
                    throw;

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                // User-a mesaj (500-də təhlükəsiz ümumi mesaj)
                var userMessage = statusCode == 500
                    ? "Daxili xəta baş verdi. Zəhmət olmasa sonra yenidən cəhd edin."
                    : ex.Message;

                // ServiceResultVoid formatında payload
                // ErrorId-ni Errors içində veririk ki user support-a göndərə bilsin
                var result = ServiceResultVoid.Fail(
                    errors: new List<string>
                    {
                        userMessage,
                        $"ErrorId: {errorId}"
                    },
                    message: userMessage,
                    statusCode: statusCode
                );

                // Dev-də debug üçün exception mesajını da əlavə etmək istəsən:
                // (hazırda yuxarıda userMessage 400-lərdə ex.Message-dir, 500-lərdə ümumidir)

                var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(json);
            }
        }
    }
}
