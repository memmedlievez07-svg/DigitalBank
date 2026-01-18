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

            if (dto.Amount <= 0)
                return ServiceResultVoid.Fail("Amount must be > 0", 400);

            var me = _current.UserId!;

            // tracking lazımdır
            var senderWallet = await _uow.WalletReadRepository.Table
                .FirstOrDefaultAsync(w => w.UserId == me);

            if (senderWallet == null)
                return ServiceResultVoid.Fail("Sender wallet not found", 404);

            if (senderWallet.Status != WalletStatus.Active)
                return ServiceResultVoid.Fail("Sender wallet is not active", 400);

            // Qeyd: burada sən hazırda "ReceiverCardNumber" yazmısan, amma Id kimi istifadə edirsən.
            // Səndə necədirsə elə saxladım:
            var receiverWallet = await _uow.WalletReadRepository.Table
                .FirstOrDefaultAsync(w => w.Id == dto.ReceiverCardNumber);

            if (receiverWallet == null)
                return ServiceResultVoid.Fail("Receiver wallet not found", 404);

            if (receiverWallet.Status != WalletStatus.Active)
                return ServiceResultVoid.Fail("Receiver wallet is not active", 400);

            if (senderWallet.Id == receiverWallet.Id)
                return ServiceResultVoid.Fail("Cannot transfer to same wallet", 400);

            if (senderWallet.Balance < dto.Amount)
                return ServiceResultVoid.Fail("Insufficient balance", 400);

            await using var tr = await _uow.BeginTransactionAsync();

            try
            {
                // balances
                senderWallet.Balance -= dto.Amount;
                receiverWallet.Balance += dto.Amount;

                _uow.WalletWriteRepository.Update(senderWallet);
                _uow.WalletWriteRepository.Update(receiverWallet);

                // transaction
                var tx = new BankTransaction
                {
                    SenderWalletId = senderWallet.Id,
                    ReceiverWalletId = receiverWallet.Id,
                    Amount = dto.Amount,
                    FeeAmount = 0m,
                    Type = TransactionType.Transfer,
                    Status = TransactionStatus.Completed,
                    ReferenceNo = $"TRX-{Guid.NewGuid():N}".ToUpperInvariant(),
                    Description = dto.Description
                };

                await _uow.BankTransactionWriteRepository.AddAsync(tx);

                // notif-lar (tx.Id SaveChanges-dən sonra gəlir, amma EF tracking ilə ilişdirəcək)
                var incoming = new Notification
                {
                    UserId = receiverWallet.UserId,
                    Type = NotificationType.IncomingTransfer,
                    Title = "Incoming transfer",
                    Body = $"You received {dto.Amount} {receiverWallet.Currency}",
                    IsRead = false,
                    RelatedTransactionId = null // aşağıda tx.Id ilə set edirik
                };

                var outgoing = new Notification
                {
                    UserId = senderWallet.UserId,
                    Type = NotificationType.OutgoingTransfer,
                    Title = "Outgoing transfer",
                    Body = $"You sent {dto.Amount} {senderWallet.Currency}",
                    IsRead = false,
                    RelatedTransactionId = null
                };

                await _uow.NotificationWriteRepository.AddAsync(incoming);
                await _uow.NotificationWriteRepository.AddAsync(outgoing);

                // 1 dəfə commit (hamısını yazır)
                await _uow.CommitAsync();

                // artıq tx.Id var
                incoming.RelatedTransactionId = tx.Id;
                outgoing.RelatedTransactionId = tx.Id;

                _uow.NotificationWriteRepository.Update(incoming);
                _uow.NotificationWriteRepository.Update(outgoing);

                await _uow.CommitAsync();
                await tr.CommitAsync();

                // Push (DB uğurlu olduqdan sonra)
                await _push.PushToUserAsync(receiverWallet.UserId, new { title = incoming.Title, body = incoming.Body, type = (int)incoming.Type });
                await _push.PushUnreadCountChangedAsync(receiverWallet.UserId);

                await _push.PushToUserAsync(senderWallet.UserId, new { title = outgoing.Title, body = outgoing.Body, type = (int)outgoing.Type });
                await _push.PushUnreadCountChangedAsync(senderWallet.UserId);

                return ServiceResultVoid.Ok("Transfer completed");
            }
            catch
            {
                await tr.RollbackAsync();
                return ServiceResultVoid.Fail("Transfer failed", 500);
            }
        }
    }
}
