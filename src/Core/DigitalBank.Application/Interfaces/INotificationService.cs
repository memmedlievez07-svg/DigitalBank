using DigitalBank.Application.Dtos.AdminDashBoardDtos;
using DigitalBank.Application.Results;

namespace DigitalBank.Application.Interfaces
{
    public interface INotificationService
    {
        Task<ServiceResult<PagedResult<NotificationListItemDto>>> GetMyAsync(NotificationFilterDto filter);
        Task<ServiceResultVoid> MarkReadAsync(int id);
        Task<ServiceResult<int>> GetUnreadCountAsync();
    }
}
