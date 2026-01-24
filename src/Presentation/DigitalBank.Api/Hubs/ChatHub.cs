using Microsoft.AspNetCore.SignalR;

namespace DigitalBank.Api.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // NameUserIdProvider sayəsində bura dolacaq
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                // İstifadəçini öz ID-si adına olan qrupa salırıq
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}