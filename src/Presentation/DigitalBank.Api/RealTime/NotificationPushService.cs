using DigitalBank.Application.Interfaces;
using DigitalBank.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DigitalBank.Api.Realtime
{
    public class NotificationPushService : INotificationPushService
    {
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationPushService(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task PushToUserAsync(string userId, object notification)
        {
            // Clients.User istifadə etmək ən təhlükəsizidir (IUserIdProvider varsa)
            await _hub.Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }

        public async Task PushUnreadCountChangedAsync(string userId)
        {
            await _hub.Clients.Group(userId)
                .SendAsync("notification:unreadCountChanged");
        }
        public async Task PushToAllAsync(object payload)
        {
            // Bu sətir serverdəki bütün qoşulu istifadəçilərə tək bir əmrlə mesaj göndərir
            await _hub.Clients.All.SendAsync("ReceiveNotification", payload);
        }
    }
}
