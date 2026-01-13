using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Results;
using DigitalBank.Domain.Enums;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace DigitalBank.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task<ServiceResultVoid> WriteAsync(
            AuditActionType actionType,
            bool isSuccess,
            string? description,
            string? detailsJson = null,
            int? relatedTransactionId = null,
            string? overrideUserId = null
            );
        Task<ServiceResult<PagedResult<AuditLogListItemDto>>> SearchAsync(AuditLogFilterDto filter);
    }
}
