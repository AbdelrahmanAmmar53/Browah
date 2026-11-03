using System.Text.Json;
using System.Text.Json.Nodes;
using Berwah.BL.Service.IService;
using Berwah.Hubs;
using Berwah.Localization;
using Microsoft.AspNetCore.SignalR;

namespace Berwah.Services
{
    public class SignalRNotificationPusher : INotificationPusher
    {
        private readonly IHubContext<NotificationHub> _hub;
        private readonly FcmSender _fcm;
        public SignalRNotificationPusher(IHubContext<NotificationHub> hub, FcmSender fcm)
        {
            _hub = hub;
            _fcm = fcm;
        }

        public async Task PushAsync(int userId, object payload)
        {
            try
            {
                // The push has no Accept-Language header, so it carries both languages
                // and the app shows the one the person chose.
                object toSend = payload;
                string? pushTitle = null, pushTitleEn = null, pushMessage = null, pushMessageEn = null;
                int notificationId = 0;
                if (JsonSerializer.SerializeToNode(payload) is JsonObject obj)
                {
                    var title = obj["title"]?.GetValue<string>();
                    var message = obj["message"]?.GetValue<string>();
                    obj["titleEn"] = title == null ? null : ArabicToEnglish.Translate(title);
                    obj["messageEn"] = message == null ? null : ArabicToEnglish.Translate(message);
                    toSend = obj;
                    pushTitle = title;
                    pushTitleEn = obj["titleEn"]?.GetValue<string>();
                    pushMessage = message;
                    pushMessageEn = obj["messageEn"]?.GetValue<string>();
                    try { notificationId = obj["id"]?.GetValue<int>() ?? 0; } catch { }
                }
                await _hub.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", toSend);

                // The phone gets it too when the app is closed; this must not slow the request down.
                if (pushTitle != null && pushMessage != null)
                {
                    var (a, b, c, d, e) = (pushTitle, pushTitleEn ?? "", pushMessage, pushMessageEn ?? "", notificationId);
                    _ = Task.Run(() => _fcm.SendAsync(userId, a, b, c, d, e));
                }
            }
            catch
            {
                // The notification is already stored; a failed push must never fail the request.
            }
        }
    }
}
