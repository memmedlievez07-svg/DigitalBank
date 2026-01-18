using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using DigitalBank.Domain.Entities;
using DigitalBank.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace DigitalBank.Persistence.Services
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserContext _current;
        private readonly IConfiguration _config;
        private readonly INotificationPushService _push;

        public StripePaymentService(
            IUnitOfWork uow,
            ICurrentUserContext current,
            IConfiguration config,
            INotificationPushService push)
        {
            _uow = uow;
            _current = current;
            _config = config;
            _push = push;
        }

        public async Task<ServiceResult<string>> CreateCheckoutSessionAsync(decimal amount, string currency = "azn")
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResult<string>.Fail("Unauthorized", 401);

            if (amount <= 0)
                return ServiceResult<string>.Fail("Amount must be greater than 0", 400);

            // Minimum və Maximum limit
            if (amount < 1)
                return ServiceResult<string>.Fail("Minimum top-up amount is 1 AZN", 400);

            if (amount > 50000)
                return ServiceResult<string>.Fail("Maximum top-up amount is 50,000 AZN", 400);

            var userId = _current.UserId!;

            // User wallet-i yoxla
            var wallet = await _uow.WalletReadRepository.Table
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
                return ServiceResult<string>.Fail("Wallet not found", 404);

            if (wallet.Status != WalletStatus.Active)
                return ServiceResult<string>.Fail("Wallet is not active", 400);

            try
            {
                // Stripe Checkout Session yaradırıq
                var domain = _config["ApiBaseUrl"] ?? "https://localhost:7055";

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = currency.ToLower(),
                                UnitAmount = (long)(amount * 100), // Stripe qəpik istəyir
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Wallet Top-Up",
                                    Description = $"Add {amount} {currency.ToUpper()} to your wallet",
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = $"{domain}/api/client/payment/success?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{domain}/api/client/payment/cancel",
                    ClientReferenceId = userId, // User-i identify etmək üçün
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", userId },
                        { "walletId", wallet.Id.ToString() },
                        { "amount", amount.ToString() },
                        { "currency", currency }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return ServiceResult<string>.Ok(session.Url, "", 200);
            }
            catch (StripeException ex)
            {
                return ServiceResult<string>.Fail($"Stripe error: {ex.Message}", 500);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Fail($"Error creating checkout session: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResultVoid> HandleWebhookAsync(string jsonPayload, string stripeSignature)
        {
            var webhookSecret = _config["Stripe:WebhookSecret"];

            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                // Development-də webhook secret olmaya bilər, amma production-da mütləq olmalıdır
                return await ProcessEventWithoutSignatureAsync(jsonPayload);
            }

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    jsonPayload,
                    stripeSignature,
                    webhookSecret
                );

                return await ProcessStripeEventAsync(stripeEvent);
            }
            catch (StripeException)
            {
                return ServiceResultVoid.Fail("Invalid signature", 400);
            }
        }

        private async Task<ServiceResultVoid> ProcessEventWithoutSignatureAsync(string jsonPayload)
        {
            var stripeEvent = EventUtility.ParseEvent(jsonPayload);
            return await ProcessStripeEventAsync(stripeEvent);
        }

        private async Task<ServiceResultVoid> ProcessStripeEventAsync(Event stripeEvent)
        {
            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session == null)
                    return ServiceResultVoid.Fail("Invalid session data", 400);

                return await CompleteTopUpAsync(session);
            }

            // Digər event-lər üçün (hələlik ignore edirik)
            return ServiceResultVoid.Ok("Event ignored");
        }

        private async Task<ServiceResultVoid> CompleteTopUpAsync(Session session)
        {
            var userId = session.Metadata["userId"];
            var walletId = int.Parse(session.Metadata["walletId"]);
            var amount = decimal.Parse(session.Metadata["amount"]);
            var currency = session.Metadata["currency"];

            await using var tr = await _uow.BeginTransactionAsync();

            try
            {
                var wallet = await _uow.WalletReadRepository.Table
                    .FirstOrDefaultAsync(w => w.Id == walletId && w.UserId == userId);

                if (wallet == null)
                {
                    await tr.RollbackAsync();
                    return ServiceResultVoid.Fail("Wallet not found", 404);
                }

                // Balansı artır
                wallet.Balance += amount;
                _uow.WalletWriteRepository.Update(wallet);

                // Transaction yaradırıq
                var tx = new BankTransaction
                {
                    ReceiverWalletId = wallet.Id,
                    SenderWalletId = null, // External source
                    Amount = amount,
                    FeeAmount = 0m,
                    Type = TransactionType.TopUp,
                    Status = TransactionStatus.Completed,
                    ReferenceNo = $"TOPUP-{session.Id}",
                    Description = $"Stripe top-up via checkout ({session.PaymentIntentId})"
                };

                await _uow.BankTransactionWriteRepository.AddAsync(tx);

                // Notification
                var notification = new Notification
                {
                    UserId = userId,
                    Type = NotificationType.TopUp, // = 6
                    Title = "Top-Up Successful",
                    Body = $"Your wallet has been credited with {amount} {currency.ToUpper()}",
                    IsRead = false,
                    RelatedTransactionId = null
                };

                await _uow.NotificationWriteRepository.AddAsync(notification);
                await _uow.CommitAsync();

                notification.RelatedTransactionId = tx.Id;
                _uow.NotificationWriteRepository.Update(notification);
                await _uow.CommitAsync();

                await tr.CommitAsync();

                // Real-time push
                await _push.PushToUserAsync(userId, new
                {
                    title = notification.Title,
                    body = notification.Body,
                    type = (int)notification.Type
                });
                await _push.PushUnreadCountChangedAsync(userId);

                return ServiceResultVoid.Ok("Top-up completed successfully");
            }
            catch (Exception ex)
            {
                await tr.RollbackAsync();
                return ServiceResultVoid.Fail($"Failed to complete top-up: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResult<string>> GetPaymentStatusAsync(string sessionId)
        {
            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(sessionId);

                return ServiceResult<string>.Ok(session.PaymentStatus, "", 200);
            }
            catch (StripeException ex)
            {
                return ServiceResult<string>.Fail($"Error retrieving payment status: {ex.Message}", 500);
            }
        }
    }
}