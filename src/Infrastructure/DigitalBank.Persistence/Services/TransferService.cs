using DigitalBank.Application.Dtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using DigitalBank.Domain.Entities;
using DigitalBank.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class TransferService : ITransferService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserContext _current;
        private readonly INotificationPushService _push;

        public TransferService(IUnitOfWork uow, ICurrentUserContext current, INotificationPushService push)
        {
            _uow = uow;
            _current = current;
            _push = push;
        }

        public async Task<ServiceResultVoid> TransferAsync(TransferRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResultVoid.Fail("Unauthorized", 401);

            var senderId = _current.UserId!;

            using var tr = await _uow.BeginTransactionAsync();
            try
            {
                // 1. Cüzdanları tapırıq
                var senderWallet = await _uow.WalletReadRepository.Table
                    .FirstOrDefaultAsync(w => w.UserId == senderId);

                var receiverWallet = await _uow.WalletReadRepository.Table
                    .FirstOrDefaultAsync(w => w.CardNumber == dto.ReceiverCardNumber);

                // 2. Validasiyalar
                if (senderWallet == null) return ServiceResultVoid.Fail("Göndərən cüzdan tapılmadı", 404);
                if (receiverWallet == null) return ServiceResultVoid.Fail("Qəbul edən cüzdan tapılmadı", 404);
                if (senderWallet.Id == receiverWallet.Id) return ServiceResultVoid.Fail("Özünüzə pul göndərə bilməzsiniz", 400);
                if (senderWallet.Balance < dto.Amount) return ServiceResultVoid.Fail("Balansınızda kifayət qədər vəsait yoxdur", 400);

                // 3. Balansların yenilənməsi
                senderWallet.Balance -= dto.Amount;
                receiverWallet.Balance += dto.Amount;

                _uow.WalletWriteRepository.Update(senderWallet);
                _uow.WalletWriteRepository.Update(receiverWallet);

                // 4. Əməliyyatın (Transaction) qeydi - Dashboard-da görünməsi üçün
                var tx = new BankTransaction
                {
                    SenderWalletId = senderWallet.Id,
                    ReceiverWalletId = receiverWallet.Id,
                    Amount = dto.Amount,
                    ReferenceNo = $"TRF-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Description = dto.Description ?? "Pul köçürməsi",
                    Status = TransactionStatus.Completed,
                    Type = TransactionType.Transfer,
                    CreatedDate = DateTime.UtcNow // Dashboard-da tarixə görə sıralama üçün vacibdir
                };
                await _uow.BankTransactionWriteRepository.AddAsync(tx);

                // 5. BİLDİRİŞİ BAZAYA YAZMAQ - Bildirişlər bölməsi boş qalmasın deyə
                // Əgər Notification modelin varsa bu hissəni işlət:
                var notification = new DigitalBank.Domain.Entities.Notification
                {
                    UserId = receiverWallet.UserId,
                    Title = "Mədaxil!",
                    Body = $"{dto.Amount} AZN vəsait daxil oldu.", // Modelində adı 'Body' ola bilər
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                };
                await _uow.NotificationWriteRepository.AddAsync(notification);

                // 6. Dəyişiklikləri yadda saxla
                await _uow.CommitAsync();
                await tr.CommitAsync();

                // 7. Real-time Bildiriş (SignalR) - Pop-up üçün
                await _push.PushToUserAsync(receiverWallet.UserId, new
                {
                    title = "Mədaxil!",
                    body = $"{dto.Amount} AZN vəsait daxil oldu.",
                    type = (int)NotificationType.IncomingTransfer
                });

                return ServiceResultVoid.Ok("Transfer uğurla tamamlandı");
            }
            catch (Exception ex)
            {
                await tr.RollbackAsync();
                return ServiceResultVoid.Fail("Transfer zamanı xəta: " + ex.Message, 500);
            }
        }
        public async Task<ServiceResult<List<RecentTransferDto>>> GetRecentTransfersAsync()
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResult<List<RecentTransferDto>>.Fail("Unauthorized", 401);

            var senderId = _current.UserId;

            // 1. Mənim göndərdiyim transferləri tapırıq
            // SenderWallet mənimdirsə, ReceiverWallet-in sahibini tapmalıyıq
            var recentUsers = await _uow.BankTransactionReadRepository.Table
                .Where(t => t.SenderWallet.UserId == senderId && t.Type == TransactionType.Transfer)
                .OrderByDescending(t => t.CreatedDate)
                .Select(t => new RecentTransferDto
                {
                    UserId = t.ReceiverWallet.UserId,
                    FirstName = t.ReceiverWallet.User.FirstName,
                    LastName = t.ReceiverWallet.User.LastName,
                    CardNumber = t.ReceiverWallet.CardNumber
                })
                .Distinct() // Eyni adama çox göndərmişəmsə, adı 1 dəfə çıxsın
                .Take(5)    // Son 5 nəfər kifayətdir
                .ToListAsync();

            return ServiceResult<List<RecentTransferDto>>.Ok(recentUsers);
        }

        // TransferService.cs daxilində GetDashboardDataAsync metodunu bu hissə ilə yenilə:
        public async Task<ServiceResult<DashboardDto>> GetDashboardDataAsync()
        {
            var userId = _current.UserId;
            if (string.IsNullOrEmpty(userId)) return ServiceResult<DashboardDto>.Fail("Unauthorized", 401);

            var wallet = await _uow.WalletReadRepository.Table
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null) return ServiceResult<DashboardDto>.Fail("Cüzdan tapılmadı", 404);

            var transactions = await _uow.BankTransactionReadRepository.Table
                .Include(t => t.SenderWallet)
                .Include(t => t.ReceiverWallet)
                .Where(t => t.SenderWalletId == wallet.Id || t.ReceiverWalletId == wallet.Id)
                .OrderByDescending(t => t.CreatedDate) // Bazadakı sütun adın CreatedDate-dir
                .Take(10) // 5 yox, 10 dənə gətirək ki siyahı dolsun
                .ToListAsync();

            var dashboard = new DashboardDto
            {
                TotalBalance = wallet.Balance,
                Currency = wallet.Currency ?? "AZN",
                RecentTransactions = transactions.Select(t => new TransactionListItemDto
                {
                    Id = t.Id,
                    ReferenceNo = t.ReferenceNo,
                    Amount = t.Amount,
                    Type = (int)t.Type,
                    Status = (int)t.Status,
                    Description = t.Description,
                    CreatedDateUtc = t.CreatedDate, // Frontend bu sahəni gözləyir
                    SenderWalletId = t.SenderWalletId,
                    ReceiverWalletId = t.ReceiverWalletId
                }).ToList()
            };

            return ServiceResult<DashboardDto>.Ok(dashboard);
        }

        public async Task<ServiceResult<List<TransactionDetailDto>>> GetTransactionHistoryAsync()
        {
            var userId = _current.UserId;
            if (string.IsNullOrEmpty(userId))
                return ServiceResult<List<TransactionDetailDto>>.Fail("Unauthorized", 401);

            // 1. İstifadəçinin cüzdanını tapırıq
            var wallet = await _uow.WalletReadRepository.Table
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
                return ServiceResult<List<TransactionDetailDto>>.Fail("Cüzdan tapılmadı", 404);

            // 2. Bütün əməliyyatları (həm göndərdiyi, həm aldığı) çəkirik
            var transactions = await _uow.BankTransactionReadRepository.Table
                .Include(t => t.SenderWallet).ThenInclude(w => w.User)
                .Include(t => t.ReceiverWallet).ThenInclude(w => w.User)
                .Where(t => t.SenderWalletId == wallet.Id || t.ReceiverWalletId == wallet.Id)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            // 3. DTO-ya çeviririk (Mapping)
            var result = transactions.Select(t => {
                // 1. Mən göndərənəm?
                bool isOutgoing = t.SenderWalletId == wallet.Id;

                // 2. Qarşı tərəf kimdir? 
                // Mən göndərirəmsə qarşı tərəf Receiver-dir, mən alıramsa Sender-dir.
                var counterpartyWallet = isOutgoing ? t.ReceiverWallet : t.SenderWallet;

                return new TransactionDetailDto
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Description = t.Description ?? "Pul köçürməsi",
                    ReferenceNo = t.ReferenceNo ?? "REF-000",
                    CreatedDate = t.CreatedDate,
                    Type = isOutgoing ? 1 : 0,

                    // NULL CHECK - Ən kritik hissə buradır
                    CounterpartyName = counterpartyWallet != null && counterpartyWallet.User != null
                        ? $"{counterpartyWallet.User.FirstName} {counterpartyWallet.User.LastName}"
                        : "Sistem Əməliyyatı", 

                    CounterpartyCard = counterpartyWallet?.CardNumber ?? "N/A"
                };
            }).ToList();

            return ServiceResult<List<TransactionDetailDto>>.Ok(result);
        }

    }
}
