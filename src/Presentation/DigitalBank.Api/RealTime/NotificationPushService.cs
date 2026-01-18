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

        public async Task PushToUserAsync(string userId, object payload)
        {
            await _hub.Clients.Group(userId)
                .SendAsync("notification:new", payload);
        }

        public async Task PushUnreadCountChangedAsync(string userId)
        {
            await _hub.Clients.Group(userId)
                .SendAsync("notification:unreadCountChanged");
        }
    }
}
