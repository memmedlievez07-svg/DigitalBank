namespace DigitalBank.Application.Interfaces
{
    public interface INotificationPushService
    {
        Task PushToUserAsync(string userId, object payload);
        Task PushUnreadCountChangedAsync(string userId);
    }
}
