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

        public async Task<ServiceResult<object>> CreateCheckoutSessionAsync(decimal amount, string currency = "azn")
        {
            // 1. İstifadəçi yoxlanışı
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResult<object>.Fail("Unauthorized", 401);

            // 2. Məbləğ yoxlanışı
            if (amount <= 0)
                return ServiceResult<object>.Fail("Məbləğ 0-dan böyük olmalıdır", 400);

            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(amount * 100), // Qəpiyə çevrilmə
                        Currency = currency.ToLower(),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "DigitalBank Balans Artımı",
                            Description = $"{amount} {currency.ToUpper()} məbləğində yükləmə"
                        },
                    },
                    Quantity = 1,
                },
            },
                    Mode = "payment",
                    // SuccessUrl və CancelUrl config-dən götürülür
                    SuccessUrl = _config["Stripe:SuccessUrl"] + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = _config["Stripe:CancelUrl"],
                    Metadata = new Dictionary<string, string>
            {
                { "UserId", _current.UserId }, // Webhook-da balansı artırmaq üçün vacibdir
                { "Amount", amount.ToString() }
            }
                };

                var service = new SessionService();
                Session session = await service.CreateAsync(options);

                // Frontend-in gözlədiyi formatda (object) qaytarırıq
                return ServiceResult<object>.Ok(new
                {
                    SessionUrl = session.Url,
                    SessionId = session.Id
                }, "Checkout session created", 200);
            }
            catch (StripeException ex)
            {
                // Stripe tərəfindən gələn xüsusi xətalar (məs: yanlış API key)
                return ServiceResult<object>.Fail($"Stripe Xətası: {ex.Message}", 500);
            }
            catch (Exception ex)
            {
                return ServiceResult<object>.Fail($"Gözlənilməz xəta: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResultVoid> HandleWebhookAsync(string json, string stripeSignature)
        {
            var webhookSecret = _config["Stripe:WebhookSecret"];
            try
            {
                // Stripe'dan gələn datanı doğrulayırıq
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

                // 'Events' xətasını buradakı 'Stripe.Events' ilə həll edirik
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                    // Metadata-dan UserId-ni alırıq (Session yaradanda qoyduğumuz)
                    var userId = session?.Metadata["UserId"];
                    var amount = (decimal)(session?.AmountTotal ?? 0) / 100;

                    if (string.IsNullOrEmpty(userId))
                        return ServiceResultVoid.Fail("UserId not found in session metadata", 400);

                    // İndi yeni yazdığımız overload-u çağırırıq
                    return await CompleteTopUpAsync(userId, amount);
                }

                return ServiceResultVoid.Ok();
            }
            catch (StripeException ex)
            {
                return ServiceResultVoid.Fail($"Stripe Webhook Error: {ex.Message}", 400);
            }
        }

        // BU METODU YENİLƏDİK - İndi həm SessionId, həm də birbaşa UserId/Amount qəbul edə bilir
        public async Task<ServiceResultVoid> CompleteTopUpAsync(string userId, decimal amount)
        {
            using var tr = await _uow.BeginTransactionAsync();
            try
            {
                // 1. Cüzdanı tapırıq
                var wallet = await _uow.WalletReadRepository.Table
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                    return ServiceResultVoid.Fail("Wallet not found", 404);

                // 2. Balansı artırırıq
                wallet.Balance += amount;
                _uow.WalletWriteRepository.Update(wallet);

                // 3. Bank əməliyyatı (Transaction) yaradırıq
                var tx = new BankTransaction
                {
                    ReceiverWalletId = wallet.Id,
                    Amount = amount,
                    Type = TransactionType.TopUp,
                    Status = TransactionStatus.Completed,
                    ReferenceNo = $"STP-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Description = "Stripe vasitəsilə balans artımı"
                };
                await _uow.BankTransactionWriteRepository.AddAsync(tx);
                await _uow.CommitAsync();

                // 4. Bildiriş yaradırıq
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "Balans artırıldı",
                    Body = $"{amount} {wallet.Currency} məbləğində vəsait balansınıza yükləndi.",
                    Type = NotificationType.TopUp,
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow,
                    RelatedTransactionId = tx.Id
                };
                await _uow.NotificationWriteRepository.AddAsync(notification);
                await _uow.CommitAsync();

                await tr.CommitAsync();

                // 5. SignalR ilə canlı məlumat göndəririk
                await _push.PushToUserAsync(userId, new
                {
                    title = notification.Title,
                    body = notification.Body,
                    type = (int)notification.Type,
                    amount = amount // Frontend balansı yeniləsin deyə
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