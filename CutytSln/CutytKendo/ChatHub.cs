﻿using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CutytKendoWeb
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

    }
}
