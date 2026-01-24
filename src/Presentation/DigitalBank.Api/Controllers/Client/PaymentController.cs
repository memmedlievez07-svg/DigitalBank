using Microsoft.AspNetCore.Mvc;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Dtos; // DTO-lar buradadırsa saxla, yoxsa sil
using System.IO;

namespace DigitalBank.WebAPI.Controllers
{
    [ApiController]
    // Frontend-in axtardığı "api/client/payment" route-u:
    [Route("api/client/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IStripePaymentService _stripeService;

        public PaymentController(IStripePaymentService stripeService)
        {
            _stripeService = stripeService;
        }

        // 1. Stripe Sessiyasını Başladan Metod (Top-up üçün mütləqdir)
        [HttpPost("create-checkout")]
        public async Task<IActionResult> CreateCheckout([FromBody] TopUpRequest request)
        {
            // Servisindəki metodun adını yoxla (CreateCheckoutSessionAsync və ya oxşar)
            var result = await _stripeService.CreateCheckoutSessionAsync(request.Amount);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // 2. Stripe-dan gələn bildirişlər (Webhook)
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"];

            var result = await _stripeService.HandleWebhookAsync(json, signature);

            if (!result.Success)
            {
                return BadRequest();
            }

            return Ok();
        }
    }

    // Bu DTO bəlkə başqa yerdədir, amma xəta çıxmasın deyə bura da əlavə edirəm
    public class TopUpRequest
    {
        public decimal Amount { get; set; }
    }
}