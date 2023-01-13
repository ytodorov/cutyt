using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Cutyt.Core.Hubs
{
    public class ChatHub : Hub
    {
        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message + " (echo from server)");
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("setSignalrId", Context.ConnectionId);
            await base.OnConnectedAsync();
        }
    }
}
