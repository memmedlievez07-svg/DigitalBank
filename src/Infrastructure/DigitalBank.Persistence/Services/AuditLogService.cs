using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Domain.Entities;
using DigitalBank.Domain.Enums;
using DigitalBank.Persistence.Dal;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services.Audit
{
    public class AuditLogService : IAuditLogService
    {
        private readonly DigitalBankDbContext _db;
        private readonly ICurrentUserContext _current;

        public AuditLogService(DigitalBankDbContext db, ICurrentUserContext current)
        {
            _db = db;
            _current = current;
        }

        public async Task<ServiceResultVoid> WriteAsync(
            AuditActionType actionType,
            bool isSuccess,
            string? description,
            string? detailsJson = null,
            int? relatedTransactionId = null,
             string? overrideUserId = null)
        {
            var log = new AuditLog
            {
                UserId = overrideUserId ?? _current.UserId,   
                ActionType = actionType,
                IsSuccess = isSuccess,
                Description = description,
                DetailsJson = detailsJson,
                IpAddress = _current.IpAddress,
                UserAgent = _current.UserAgent,
                RelatedTransactionId = relatedTransactionId,
                CorrelationId = _current.CorrelationId
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();

            return ServiceResultVoid.Ok();
        }
        public async Task<ServiceResult<PagedResult<AuditLogListItemDto>>> SearchAsync(AuditLogFilterDto filter)
        {
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : filter.PageSize;
            if (pageSize > 200) pageSize = 200;

            var q = _db.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.UserId))
                q = q.Where(x => x.UserId == filter.UserId);

            if (filter.ActionType.HasValue)
                q = q.Where(x => x.ActionType == filter.ActionType.Value);

            if (filter.IsSuccess.HasValue)
                q = q.Where(x => x.IsSuccess == filter.IsSuccess.Value);

            if (!string.IsNullOrWhiteSpace(filter.CorrelationId))
                q = q.Where(x => x.CorrelationId == filter.CorrelationId);

            if (!string.IsNullOrWhiteSpace(filter.IpAddress))
                q = q.Where(x => x.IpAddress == filter.IpAddress);

            if (filter.RelatedTransactionId.HasValue)
                q = q.Where(x => x.RelatedTransactionId == filter.RelatedTransactionId.Value);

            if (filter.FromUtc.HasValue)
                q = q.Where(x => x.CreatedDate >= filter.FromUtc.Value);

            if (filter.ToUtc.HasValue)
                q = q.Where(x => x.CreatedDate <= filter.ToUtc.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                q = q.Where(x =>
                    (x.Description != null && x.Description.ToLower().Contains(s)) ||
                    (x.DetailsJson != null && x.DetailsJson.ToLower().Contains(s)) ||
                    (x.UserId != null && x.UserId.ToLower().Contains(s)) ||
                    (x.IpAddress != null && x.IpAddress.ToLower().Contains(s)) ||
                    (x.UserAgent != null && x.UserAgent.ToLower().Contains(s)) ||
                    (x.CorrelationId != null && x.CorrelationId.ToLower().Contains(s))
                );
            }

            var total = await q.CountAsync();

            var items = await q.OrderByDescending(x => x.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AuditLogListItemDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    ActionType = x.ActionType,
                    IsSuccess = x.IsSuccess,
                    RelatedTransactionId = x.RelatedTransactionId,
                    Description = x.Description,
                    DetailsJson = x.DetailsJson,
                    IpAddress = x.IpAddress,
                    UserAgent = x.UserAgent,
                    CorrelationId = x.CorrelationId,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();

            return ServiceResult<PagedResult<AuditLogListItemDto>>.Ok(new PagedResult<AuditLogListItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

    }
}
