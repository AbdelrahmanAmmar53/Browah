using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Hubs
{
    public class ChatHub:Hub
    {
        public async Task SendMessageToSupport(string message)
        {
            var userName = Context.User?.Identity?.Name ?? "عميل";
            
            await Clients.Group("SupportAgents").SendAsync("ReceiveUserMessage", userName, message);
        }

        public async Task JoinSupportGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SupportAgents");
        }
    }
}