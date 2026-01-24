using DigitalBank.Application.Dtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserContext _current;

        public WalletService(IUnitOfWork uow, ICurrentUserContext current)
        {
            _uow = uow;
            _current = current;
        }

        public async Task<ServiceResult<WalletMeDto>> GetMyWalletAsync()
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResult<WalletMeDto>.Fail("Unauthorized", 401);

            var userId = _current.UserId!;

            // 1. İlk olaraq cüzdanı və tranzaksiyaları (lazımi məlumatlarla) çəkirik
            var walletEntity = await _uow.WalletReadRepository.Table
                .AsNoTracking()
                .Include(x => x.SentTransactions)
                    .ThenInclude(t => t.ReceiverWallet.User)
                .Include(x => x.ReceivedTransactions)
                    .ThenInclude(t => t.SenderWallet.User)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (walletEntity == null)
                return ServiceResult<WalletMeDto>.Fail("Wallet not found", 404);

            // 2. Yaddaşda (In-Memory) DTO-ya çevirmə və birləşdirmə edirik
            var sent = walletEntity.SentTransactions.Select(t => new TransactionDetailDto
            {
                Id = t.Id,
                Amount = -t.Amount,
                Description = t.Description,
                ReferenceNo = t.ReferenceNo,
                CreatedDate = t.CreatedDate,
                Type = 1,
                CounterpartyName = t.ReceiverWallet?.User != null
                    ? $"{t.ReceiverWallet.User.FirstName} {t.ReceiverWallet.User.LastName}"
                    : "Naməlum",
                CounterpartyCard = t.ReceiverWallet?.CardNumber ?? "****"
            });

            var received = walletEntity.ReceivedTransactions.Select(t => new TransactionDetailDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Description = t.Description,
                ReferenceNo = t.ReferenceNo,
                CreatedDate = t.CreatedDate,
                Type = 0,
                CounterpartyName = t.SenderWallet?.User != null
                    ? $"{t.SenderWallet.User.FirstName} {t.SenderWallet.User.LastName}"
                    : "Naməlum",
                CounterpartyCard = t.SenderWallet?.CardNumber ?? "****"
            });

            // 3. Birləşdir və sırala
            var allTransactions = sent.Concat(received)
                .OrderByDescending(t => t.CreatedDate)
                .Take(15)
                .ToList();

            var dto = new WalletMeDto
            {
                Id = walletEntity.Id,
                CardNumber = walletEntity.CardNumber,
                Balance = walletEntity.Balance,
                Currency = walletEntity.Currency,
                Status = (int)walletEntity.Status,
                Transactions = allTransactions
            };

            return ServiceResult<WalletMeDto>.Ok(dto);
        }
    }
}
