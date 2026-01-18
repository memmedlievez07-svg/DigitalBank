using DigitalBank.Api.Controllers.Base;
using DigitalBank.Application.Dtos;
using DigitalBank.Application.Dtos.Stripe;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalBank.Api.Controllers.Client
{
    [Route("api/client/payment")]
    public class PaymentController : ApiControllerBase
    {
        private readonly IStripePaymentService _stripeService;

        public PaymentController(IStripePaymentService stripeService)
        {
            _stripeService = stripeService;
        }

        /// <summary>
        /// Stripe Checkout yaradır və URL qaytarır
        /// </summary>
        [Authorize]
        [HttpPost("create-checkout")]
        public async Task<IActionResult> CreateCheckout([FromBody] TopUpRequestDto dto)
        {
            var result = await _stripeService.CreateCheckoutSessionAsync(dto.Amount, dto.Currency);

            if (!result.Success)
                return FromResult(result);

            var response = new TopUpResponseDto
            {
                CheckoutUrl = result.Data!,
                SessionId = result.Data!.Split("session_id=").LastOrDefault() ?? ""
            };

            return Ok(new { success = true, data = response });
        }

        /// <summary>
        /// Stripe Webhook - ödəniş uğurlu olanda avtomatik çağırılır
        /// </summary>
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

            var result = await _stripeService.HandleWebhookAsync(json, stripeSignature);

            if (!result.Success)
                return BadRequest(new { error = result.Message });

            return Ok();
        }

        /// <summary>
        /// Success redirect endpoint (Stripe-dan qayıdanda)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] string session_id)
        {
            if (string.IsNullOrWhiteSpace(session_id))
                return BadRequest("Session ID is required");

            var statusResult = await _stripeService.GetPaymentStatusAsync(session_id);

            if (!statusResult.Success)
                return BadRequest(statusResult.Message);

            // Frontend-ə redirect (burada frontend URL-inizi yazın)
            var frontendUrl = "http://localhost:5173"; // CHANGE THIS
            return Redirect($"{frontendUrl}/payment/success?session_id={session_id}");
        }

        /// <summary>
        /// Cancel redirect endpoint (user ödənişi cancel edərsə)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("cancel")]
        public IActionResult PaymentCancel()
        {
            var frontendUrl = "http://localhost:5173"; // CHANGE THIS
            return Redirect($"{frontendUrl}/payment/cancel");
        }

        /// <summary>
        /// Payment status yoxlamaq üçün
        /// </summary>
        [Authorize]
        [HttpGet("status/{sessionId}")]
        public async Task<IActionResult> GetPaymentStatus(string sessionId)
        {
            var result = await _stripeService.GetPaymentStatusAsync(sessionId);
            return FromResult(result);
        }
    }
}