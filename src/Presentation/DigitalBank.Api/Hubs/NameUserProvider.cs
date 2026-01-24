using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DigitalBank.Api.Hubs
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Token-dəki NameIdentifier və ya 'sub' claim-ini tapırıq
            var user = connection.User;
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user?.FindFirst("sub")?.Value;
        }
    }
}
