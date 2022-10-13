using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalRApplication
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task Send(string message, string userName)
        {
            await Clients.All.SendAsync("Receive", message, userName);
        }
        [Authorize(Roles = "Admin")]
        public async Task Notify(string message)
        {
            await Clients.All.SendAsync("Receive", message, "Administrator");
        }
    }
}
