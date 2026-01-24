using DigitalBank.Application.Dtos.AdminDashBoardDtos.Wallet;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using DigitalBank.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class AdminWalletService : IAdminWalletService
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuditLogService _audit;

        public AdminWalletService(IUnitOfWork uow, IAuditLogService audit)
        {
            _uow = uow;
            _audit = audit;
        }

        public async Task<ServiceResult<PagedResult<AdminWalletListItemDto>>> ListAsync(AdminWalletFilterDto filter)
        {
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;
            if (pageSize > 200) pageSize = 200;

            var q = _uow.WalletReadRepository.Table
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.UserId))
                q = q.Where(w => w.UserId == filter.UserId);

            if (!string.IsNullOrWhiteSpace(filter.CardNumber))
                q = q.Where(w => w.CardNumber == filter.CardNumber);

            if (filter.Status.HasValue)
                q = q.Where(w => w.Status == filter.Status.Value);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(w => w.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new AdminWalletListItemDto
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    CardNumber = w.CardNumber,
                    Balance = w.Balance,
                    Currency = w.Currency,
                    Status = w.Status,
                    CreatedDate = w.CreatedDate,
                    UserFullName = w.User.FirstName + " " + w.User.LastName,
                })
                .ToListAsync();

            return ServiceResult<PagedResult<AdminWalletListItemDto>>.Ok(new PagedResult<AdminWalletListItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<ServiceResultVoid> UpdateStatusAsync(int walletId, UpdateWalletStatusDto dto)
        {
            // tracking lazımdır (update edirik)
            var w = await _uow.WalletReadRepository.Table
                .FirstOrDefaultAsync(x => x.Id == walletId);

            if (w == null)
                return ServiceResultVoid.Fail("Wallet not found", 404);

            w.Status = dto.Status;

            // əgər Table-dan tracking entity götürmüsənsə, Update çağırmaq məcburi deyil.
            // Amma səndə pattern "WriteRepository.Update"dirsə, bunu saxla:
            _uow.WalletWriteRepository.Update(w);

            await _uow.CommitAsync();

            // Audit
            await _audit.WriteAsync(
                AuditActionType.Adjustment,
                true,
                "Wallet status changed",
                detailsJson: $"{{\"walletId\":{walletId},\"newStatus\":{(int)dto.Status}}}"
            );

            return ServiceResultVoid.Ok("Wallet status updated");
        }
    }
}
