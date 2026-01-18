using DigitalBank.Api.Hubs;
using DigitalBank.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DigitalBank.Api.Realtime
{
    public class ChatPushService : IChatPushService
    {
        private readonly IHubContext<ChatHub> _hub;

        public ChatPushService(IHubContext<ChatHub> hub)
        {
            _hub = hub;
        }

        public Task PushMessageAsync(string receiverUserId, object payload)
            => _hub.Clients.Group(receiverUserId).SendAsync("chat:newMessage", payload);
    }
}
