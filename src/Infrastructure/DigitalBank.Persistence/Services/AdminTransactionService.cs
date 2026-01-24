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
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;
            if (pageSize > 200) pageSize = 200;

            var q = _uow.BankTransactionReadRepository.Table
                .AsNoTracking()
                .AsQueryable();

            // ... (Filter hissələri eyni qalır, onları dəyişməyə ehtiyac yoxdur) ...

            var totalCount = await q.CountAsync();

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

            var walletIds = txPage
                .SelectMany(x => new int?[] { x.SenderWalletId, x.ReceiverWalletId })
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            // BURADA DƏYİŞİKLİK EDİRİK: UserId ilə yanaşı FirstName və LastName-i də çəkirik
            var walletUserMap = await _uow.WalletReadRepository.Table
                .AsNoTracking()
                .Where(w => walletIds.Contains(w.Id))
                .Select(w => new {
                    w.Id,
                    w.UserId,
                    FullName = w.User.FirstName + " " + w.User.LastName // Navigation property vasitəsilə
                })
                .ToDictionaryAsync(x => x.Id, x => new { x.UserId, x.FullName });

            var items = txPage.Select(t => new AdminTransactionListItemDto
            {
                Id = t.Id,
                ReferenceNo = t.ReferenceNo,
                Amount = t.Amount,
                FeeAmount = t.FeeAmount,
                Type = (int)t.Type,
                Status = (int)t.Status,
                CreatedDate = t.CreatedDate,

                SenderWalletId = t.SenderWalletId,
                // Map-dən həm ID, həm Ad çəkilir
                SenderUserId = t.SenderWalletId.HasValue && walletUserMap.TryGetValue(t.SenderWalletId.Value, out var s) ? s.UserId : null,
                SenderFullName = t.SenderWalletId.HasValue && walletUserMap.TryGetValue(t.SenderWalletId.Value, out var sf) ? sf.FullName : "Sistem",

                ReceiverWalletId = t.ReceiverWalletId,
                ReceiverUserId = t.ReceiverWalletId.HasValue && walletUserMap.TryGetValue(t.ReceiverWalletId.Value, out var r) ? r.UserId : null,
                ReceiverFullName = t.ReceiverWalletId.HasValue && walletUserMap.TryGetValue(t.ReceiverWalletId.Value, out var rf) ? rf.FullName : "Naməlum"
            }).ToList();

            return ServiceResult<PagedResult<AdminTransactionListItemDto>>.Ok(new PagedResult<AdminTransactionListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
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
