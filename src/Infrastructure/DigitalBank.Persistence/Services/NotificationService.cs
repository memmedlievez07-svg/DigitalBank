using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserContext _current;

        public NotificationService(IUnitOfWork uow, ICurrentUserContext current)
        {
            _uow = uow;
            _current = current;
        }

        public async Task<ServiceResult<PagedResult<NotificationListItemDto>>> GetMyAsync(NotificationFilterDto filter)
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResult<PagedResult<NotificationListItemDto>>.Fail("Unauthorized", 401);

            var userId = _current.UserId!;

            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;
            if (pageSize > 100) pageSize = 100;

            var q = _uow.NotificationReadRepository.Table
                .AsNoTracking()
                .Where(n => n.UserId == userId);

            if (filter.Type.HasValue)
                q = q.Where(n => n.Type == filter.Type.Value);

            if (filter.IsRead.HasValue)
                q = q.Where(n => n.IsRead == filter.IsRead.Value);

            if (filter.FromUtc.HasValue)
                q = q.Where(n => n.CreatedDate >= filter.FromUtc.Value);

            if (filter.ToUtc.HasValue)
                q = q.Where(n => n.CreatedDate <= filter.ToUtc.Value);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(n => n.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationListItemDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Body = n.Body,
                    IsRead = n.IsRead,
                    CreatedDate = n.CreatedDate,
                    ReadAt = n.ReadAt,
                    RelatedTransactionId = n.RelatedTransactionId
                })
                .ToListAsync();

            return ServiceResult<PagedResult<NotificationListItemDto>>.Ok(new PagedResult<NotificationListItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<ServiceResultVoid> MarkReadAsync(int id)
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResultVoid.Fail("Unauthorized", 401);

            var userId = _current.UserId!;

            // tracking lazımdır (update edirik)
            var n = await _uow.NotificationReadRepository.Table
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (n == null)
                return ServiceResultVoid.Fail("Notification not found", 404);

            if (!n.IsRead)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;

                _uow.NotificationWriteRepository.Update(n);
                await _uow.CommitAsync();
            }

            return ServiceResultVoid.Ok("Marked as read");
        }

        public async Task<ServiceResult<int>> GetUnreadCountAsync()
        {
            if (string.IsNullOrWhiteSpace(_current.UserId))
                return ServiceResult<int>.Fail("Unauthorized", 401);

            var userId = _current.UserId!;

            var count = await _uow.NotificationReadRepository.Table
                .AsNoTracking()
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return ServiceResult<int>.Ok(count);
        }
    }
}
