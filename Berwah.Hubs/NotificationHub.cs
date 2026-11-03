using Microsoft.AspNetCore.SignalR;

namespace Berwah.Hubs
{
    /// <summary>
    /// One connection per signed-in app. The server sends "ReceiveNotification" to
    /// <c>Clients.User(userId)</c>; the user id is the token's NameIdentifier claim.
    /// The hub itself takes no calls from the app.
    /// </summary>
    public class NotificationHub : Hub
    {
    }
}
