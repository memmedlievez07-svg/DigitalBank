using DigitalBank.Application.Results;

namespace DigitalBank.Application.Interfaces
{
    public interface IStripePaymentService
    {
        /// <summary>
        /// Stripe Checkout Session yaradır və URL qaytarır
        /// </summary>
        Task<ServiceResult<object>> CreateCheckoutSessionAsync(decimal amount, string currency = "azn");

        /// <summary>
        /// Webhook-dən gələn event-i işləyir (ödəniş uğurlu olanda balans artırır)
        /// </summary>
        Task<ServiceResultVoid> HandleWebhookAsync(string jsonPayload, string stripeSignature);

        /// <summary>
        /// Payment Intent-in statusunu yoxlayır
        /// </summary>
        Task<ServiceResult<string>> GetPaymentStatusAsync(string sessionId);
    }
}