using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Berwah.Hubs
{
    public class OrderHub : Hub
    {
        
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        // استقبال موقع الكابتن وإرساله للعميل في نفس اللحظة
        public async Task UpdateCaptainLocation(string orderId, double latitude, double longitude)
        {
            await Clients.Group($"order_{orderId}").SendAsync("ReceiveLocationUpdate", latitude, longitude);
        }
    }
}
