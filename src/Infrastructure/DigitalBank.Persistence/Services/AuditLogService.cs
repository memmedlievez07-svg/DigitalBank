using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Results;
using DigitalBank.Application.UnitOfWork;
using DigitalBank.Domain.Entities;
using DigitalBank.Domain.Enums;
using DigitalBank.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DigitalBank.Persistence.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserContext _current;

        public AuditLogService(IUnitOfWork uow, ICurrentUserContext current)
        {
            _uow = uow;
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

            await _uow.AuditLogWriteRepository.AddAsync(log);
            await _uow.CommitAsync();

            return ServiceResultVoid.Ok();
        }

        public async Task<ServiceResult<PagedResult<AuditLogListItemDto>>> SearchAsync(AuditLogFilterDto filter)
        {
            // defaults & guard
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : filter.PageSize;
            if (pageSize > 200) pageSize = 200;

            // build query (Table üstündən)
            var q = _uow.AuditLogReadRepository.Table
                .AsNoTracking()
                .ApplyFilters(filter);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(x => x.CreatedDate)
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

            var paged = new PagedResult<AuditLogListItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<AuditLogListItemDto>>.Ok(paged);
        }
    }
}
