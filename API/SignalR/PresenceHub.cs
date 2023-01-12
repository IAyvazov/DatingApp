using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using API.Extensions;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            this.tracker = tracker;
            
        }

        public override async Task OnConnectedAsync()
        {
            await this.tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

            var currentUser = await this.tracker.GetOnlineUsers();

            await Clients.All.SendAsync("GetOnlineUsers", currentUser);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await this.tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            var currentUser = await this.tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUser);

            await base.OnDisconnectedAsync(exception);
        }
    }
}