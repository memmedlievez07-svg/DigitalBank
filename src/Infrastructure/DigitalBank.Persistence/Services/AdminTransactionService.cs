using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class AdminTransactionService : IAdminTransactionService
    {
        private readonly IUnitOfWork _uow;

        public AdminTransactionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ServiceResult<PagedResult<AdminTransactionListItemDto>>> ListAsync(AdminTransactionFilterDto filter)
        {
            // defensive defaults
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;
            if (pageSize > 200) pageSize = 200;

            // Base query (ReadRepository Table üstündən)
            var q = _uow.BankTransactionReadRepository.Table
                .AsNoTracking()
                .AsQueryable();

            // Filters
            if (filter.Type.HasValue)
                q = q.Where(x => x.Type == filter.Type.Value);

            if (filter.Status.HasValue)
                q = q.Where(x => x.Status == filter.Status.Value);

            if (filter.FromUtc.HasValue)
                q = q.Where(x => x.CreatedDate >= filter.FromUtc.Value);

            if (filter.ToUtc.HasValue)
                q = q.Where(x => x.CreatedDate <= filter.ToUtc.Value);

            if (filter.MinAmount.HasValue)
                q = q.Where(x => x.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                q = q.Where(x => x.Amount <= filter.MaxAmount.Value);

            // Sender/Receiver UserId filter (Wallet.UserId vasitəsilə)
            // DbContext əvəzinə WalletReadRepository.Table
            if (!string.IsNullOrWhiteSpace(filter.SenderUserId))
            {
                var senderUserId = filter.SenderUserId.Trim();
                q = q.Where(t =>
                    t.SenderWalletId != null &&
                    _uow.WalletReadRepository.Table.Any(w => w.Id == t.SenderWalletId && w.UserId == senderUserId));
            }

            if (!string.IsNullOrWhiteSpace(filter.ReceiverUserId))
            {
                var receiverUserId = filter.ReceiverUserId.Trim();
                q = q.Where(t =>
                    t.ReceiverWalletId != null &&
                    _uow.WalletReadRepository.Table.Any(w => w.Id == t.ReceiverWalletId && w.UserId == receiverUserId));
            }

            var totalCount = await q.CountAsync();

            // Page (sənin eyni select-in)
            var txPage = await q
                .OrderByDescending(x => x.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.ReferenceNo,
                    t.Amount,
                    t.FeeAmount,
                    t.Type,
                    t.Status,
                    t.SenderWalletId,
                    t.ReceiverWalletId,
                    t.CreatedDate
                })
                .ToListAsync();

            // WalletId -> UserId mapping (1 query)
            var walletIds = txPage
                .SelectMany(x => new int?[] { x.SenderWalletId, x.ReceiverWalletId })
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var walletUserMap = await _uow.WalletReadRepository.Table
                .AsNoTracking()
                .Where(w => walletIds.Contains(w.Id))
                .Select(w => new { w.Id, w.UserId })
                .ToDictionaryAsync(x => x.Id, x => x.UserId);

            var items = txPage.Select(t => new AdminTransactionListItemDto
            {
                Id = t.Id,
                ReferenceNo = t.ReferenceNo,
                Amount = t.Amount,
                FeeAmount = t.FeeAmount,
                Type = (int)t.Type,
                Status = (int)t.Status,

                SenderWalletId = t.SenderWalletId,
                SenderUserId = t.SenderWalletId.HasValue && walletUserMap.TryGetValue(t.SenderWalletId.Value, out var su) ? su : null,

                ReceiverWalletId = t.ReceiverWalletId,
                ReceiverUserId = t.ReceiverWalletId.HasValue && walletUserMap.TryGetValue(t.ReceiverWalletId.Value, out var ru) ? ru : null,

                CreatedDate = t.CreatedDate
            }).ToList();

            var result = new PagedResult<AdminTransactionListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<AdminTransactionListItemDto>>.Ok(result);
        }

        public async Task<ServiceResult<AdminTransactionDetailsDto>> GetByIdAsync(int id)
        {
            var tx = await _uow.BankTransactionReadRepository.Table
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.ReferenceNo,
                    t.Amount,
                    t.FeeAmount,
                    t.Type,
                    t.Status,
                    t.SenderWalletId,
                    t.ReceiverWalletId,
                    t.Description,
                    t.CreatedDate,
                    t.ModifiedDate
                })
                .FirstOrDefaultAsync();

            if (tx == null)
                return ServiceResult<AdminTransactionDetailsDto>.Fail("Transaction not found", 404);

            string? senderUserId = null;
            string? receiverUserId = null;

            if (tx.SenderWalletId.HasValue)
            {
                senderUserId = await _uow.WalletReadRepository.Table
                    .AsNoTracking()
                    .Where(w => w.Id == tx.SenderWalletId.Value)
                    .Select(w => w.UserId)
                    .FirstOrDefaultAsync();
            }

            if (tx.ReceiverWalletId.HasValue)
            {
                receiverUserId = await _uow.WalletReadRepository.Table
                    .AsNoTracking()
                    .Where(w => w.Id == tx.ReceiverWalletId.Value)
                    .Select(w => w.UserId)
                    .FirstOrDefaultAsync();
            }

            var dto = new AdminTransactionDetailsDto
            {
                Id = tx.Id,
                ReferenceNo = tx.ReferenceNo,
                Amount = tx.Amount,
                FeeAmount = tx.FeeAmount,
                Type = (int)tx.Type,
                Status = (int)tx.Status,
                SenderWalletId = tx.SenderWalletId,
                SenderUserId = senderUserId,
                ReceiverWalletId = tx.ReceiverWalletId,
                ReceiverUserId = receiverUserId,
                Description = tx.Description,
                CreatedDate = tx.CreatedDate,
                ModifiedDate = tx.ModifiedDate
            };

            return ServiceResult<AdminTransactionDetailsDto>.Ok(dto);
        }
    }
}
