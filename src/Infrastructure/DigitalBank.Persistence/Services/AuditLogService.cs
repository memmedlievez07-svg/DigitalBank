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

        //public async Task<ServiceResult<PagedResult<AuditLogListItemDto>>> SearchAsync(AuditLogFilterDto filter)
        //{
        //    // defaults & guard
        //    var page = filter.Page < 1 ? 1 : filter.Page;
        //    var pageSize = filter.PageSize < 1 ? 50 : filter.PageSize;
        //    if (pageSize > 200) pageSize = 200;

        //    // build query (Table üstündən)
        //    var q = _uow.AuditLogReadRepository.Table
        //        .AsNoTracking()
        //        .ApplyFilters(filter);

        //    var total = await q.CountAsync();

        //    var items = await q
        //        .OrderByDescending(x => x.CreatedDate)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(x => new AuditLogListItemDto
        //        {
        //            Id = x.Id,
        //            UserId = x.UserId,
        //            ActionType = x.ActionType,
        //            IsSuccess = x.IsSuccess,
        //            RelatedTransactionId = x.RelatedTransactionId,
        //            Description = x.Description,
        //            DetailsJson = x.DetailsJson,
        //            IpAddress = x.IpAddress,
        //            UserAgent = x.UserAgent,
        //            CorrelationId = x.CorrelationId,
        //            CreatedDate = x.CreatedDate

        //        })
        //        .ToListAsync();

        //    var paged = new PagedResult<AuditLogListItemDto>
        //    {
        //        Items = items,
        //        TotalCount = total,
        //        Page = page,
        //        PageSize = pageSize
        //    };

        //    return ServiceResult<PagedResult<AuditLogListItemDto>>.Ok(paged);
        //}

        public async Task<ServiceResult<PagedResult<AuditLogListItemDto>>> SearchAsync(AuditLogFilterDto filter)
        {
            // 1. Defolt dəyərlər
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 50 : filter.PageSize;
            if (pageSize > 200) pageSize = 200;

            // 2. Query qurulur
            var q = _uow.AuditLogReadRepository.Table
                .AsNoTracking()
                .ApplyFilters(filter);

            var total = await q.CountAsync();

            // 3. Loqları anonim siyahı kimi çəkirik
            var rawItems = await q
                .OrderByDescending(x => x.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // Burada bazadan datanı gətiririk

            // 4. Siyahıdakı unikal UserId-ləri tapırıq
            var userIds = rawItems
             .Where(x => !string.IsNullOrEmpty(x.UserId)) // Null və ya boş olanları keçirik
             .Select(x => x.UserId!)
             .Distinct()
             .ToList();

            // 5. Bu ID-lərə uyğun Email və FullName-ləri bir sorğu ilə çəkirik
            // QEYD: 'Users' cədvəlinə müraciət üçün _uow daxilində müvafiq repository olmalıdır
            // Əgər birbaşa müraciət yoxdursa, 'WalletReadRepository.Table.Select(x => x.User)' istifadə edilə bilər
            var userMap = await _uow.WalletReadRepository.Table
            .AsNoTracking()
            .Where(w => userIds.Contains(w.UserId))
            .Select(w => new {
             w.UserId,
             w.User.Email,
              FullName = w.User.FirstName + " " + w.User.LastName
    })
    .ToDictionaryAsync(x => x.UserId, x => new { x.Email, x.FullName });

            // 6. DTO-ya çeviririk və adları map-dən oxuyuruq
            var items = rawItems.Select(x => new AuditLogListItemDto
            {
                Id = x.Id,
                UserId = x.UserId,

                // Əgər UserId null deyilsə və Map-də tapılıbsa datanı qoy, yoxsa "Sistem" yaz
                Email = !string.IsNullOrEmpty(x.UserId) && userMap.TryGetValue(x.UserId, out var u)
                ? u.Email
                : (string.IsNullOrEmpty(x.UserId) ? "system@bank.com" : "İstifadəçi tapılmadı"),

                Fullname = !string.IsNullOrEmpty(x.UserId) && userMap.TryGetValue(x.UserId, out var uf)
                ? uf.FullName
                : (string.IsNullOrEmpty(x.UserId) ? "SİSTEM ƏMƏLİYYATI" : "Naməlum İstifadəçi"),
                ActionType = x.ActionType,         
                IsSuccess = x.IsSuccess,          
                Description = x.Description,       
                CreatedDate = x.CreatedDate,      
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                CorrelationId = x.CorrelationId

                // ... digər sahələr
            }).ToList();

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
