using Berwah.DAL.App;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;

namespace Berwah.Services
{
    /// <summary>
    /// Sends a notification to the user's phones through Firebase Cloud Messaging, so it
    /// arrives even when the app is closed. Without credentials it does nothing.
    /// The service-account key is read from the FIREBASE_CREDENTIALS_JSON environment variable,
    /// or from the file named by Firebase:CredentialsFile (default firebase-service-account.json
    /// next to the app).
    /// </summary>
    public class FcmSender
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<FcmSender> _log;
        private readonly FirebaseMessaging? _messaging;

        public FcmSender(IServiceScopeFactory scopes, IWebHostEnvironment env, IConfiguration config, ILogger<FcmSender> log)
        {
            _scopes = scopes;
            _log = log;
            try
            {
                GoogleCredential? credential = null;
                var json = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON") ?? config["Firebase:CredentialsJson"];
                if (!string.IsNullOrWhiteSpace(json))
                {
                    credential = GoogleCredential.FromJson(json);
                }
                else
                {
                    var file = config["Firebase:CredentialsFile"] ?? "firebase-service-account.json";
                    var path = Path.IsPathRooted(file) ? file : Path.Combine(env.ContentRootPath, file);
                    if (File.Exists(path)) credential = GoogleCredential.FromFile(path);
                }

                if (credential == null)
                {
                    _log.LogWarning("Firebase credentials not found: push notifications to closed apps are off.");
                    return;
                }
                var app = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions { Credential = credential });
                _messaging = FirebaseMessaging.GetMessaging(app);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Firebase could not start; push notifications are off.");
            }
        }

        public async Task SendAsync(int userId, string title, string titleEn, string message, string messageEn, int notificationId, string type = "other", int refId = 0)
        {
            if (_messaging == null) return;
            try
            {
                using var scope = _scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var enabled = await db.Users.AsNoTracking().Where(u => u.Id == userId)
                    .Select(u => (bool?)u.PushNotificationEnable).FirstOrDefaultAsync();
                if (enabled == false) return;

                var tokens = await db.DeviceTokens.Where(t => t.UserId == userId).ToListAsync();
                if (tokens.Count == 0) return;

                var messages = tokens.Select(t =>
                {
                    var en = t.Language == "en";
                    return new Message
                    {
                        Token = t.Token,
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = en && !string.IsNullOrEmpty(titleEn) ? titleEn : title,
                            Body = en && !string.IsNullOrEmpty(messageEn) ? messageEn : message
                        },
                        Android = new AndroidConfig
                        {
                            Priority = Priority.High,
                            Notification = new AndroidNotification { Sound = "default", ChannelId = "berwah_notifications" }
                        },
                        Data = new Dictionary<string, string>
                        {
                            ["notificationId"] = notificationId.ToString(),
                            ["route"] = "notifications",
                            ["type"] = type,
                            ["refId"] = refId.ToString(),
                            ["title"] = title
                        }
                    };
                }).ToList();

                var response = await _messaging.SendEachAsync(messages);

                // A phone that no longer has the app (or reset its token) is forgotten.
                var dead = new List<Berwah.Models.Models.DeviceToken>();
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var r = response.Responses[i];
                    if (!r.IsSuccess && r.Exception?.MessagingErrorCode == MessagingErrorCode.Unregistered)
                        dead.Add(tokens[i]);
                }
                if (dead.Count > 0)
                {
                    db.DeviceTokens.RemoveRange(dead);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Sending a push notification failed.");
            }
        }
    }
}
