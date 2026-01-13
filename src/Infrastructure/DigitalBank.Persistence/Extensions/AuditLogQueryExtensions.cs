using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Domain.Entities;

namespace DigitalBank.Persistence.Extensions
{
    public static class AuditLogQueryExtensions
    {
        public static IQueryable<AuditLog> ApplyFilters(
            this IQueryable<AuditLog> q,
            AuditLogFilterDto f)
        {
            if (!string.IsNullOrWhiteSpace(f.UserId))
                q = q.Where(x => x.UserId == f.UserId);

            if (f.ActionType.HasValue)
                q = q.Where(x => x.ActionType == f.ActionType.Value);

            if (f.IsSuccess.HasValue)
                q = q.Where(x => x.IsSuccess == f.IsSuccess.Value);

            if (!string.IsNullOrWhiteSpace(f.CorrelationId))
                q = q.Where(x => x.CorrelationId == f.CorrelationId);

            if (!string.IsNullOrWhiteSpace(f.IpAddress))
                q = q.Where(x => x.IpAddress == f.IpAddress);

            if (f.RelatedTransactionId.HasValue)
                q = q.Where(x => x.RelatedTransactionId == f.RelatedTransactionId.Value);

            if (f.FromUtc.HasValue)
                q = q.Where(x => x.CreatedDate >= f.FromUtc.Value);

            if (f.ToUtc.HasValue)
                q = q.Where(x => x.CreatedDate <= f.ToUtc.Value);

            if (!string.IsNullOrWhiteSpace(f.Search))
            {
                var s = f.Search.Trim().ToLower();
                q = q.Where(x =>
                    (x.Description ?? "").ToLower().Contains(s) ||
                    (x.DetailsJson ?? "").ToLower().Contains(s) ||
                    (x.UserId ?? "").ToLower().Contains(s) ||
                    (x.IpAddress ?? "").ToLower().Contains(s) ||
                    (x.UserAgent ?? "").ToLower().Contains(s) ||
                    (x.CorrelationId ?? "").ToLower().Contains(s)
                );
            }

            return q;
        }
    }
}
