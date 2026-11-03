using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace Berwah.Services
{
    /// <summary>
    /// Reminds a captain before the driving license he entered runs out: 30 days before,
    /// 7 days before and on the day it expires (each once per expiry date).
    /// </summary>
    public class CaptainLicenseReminder : BackgroundService
    {
        private static readonly TimeSpan Every = TimeSpan.FromHours(6);

        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<CaptainLicenseReminder> _log;

        public CaptainLicenseReminder(IServiceScopeFactory scopes, ILogger<CaptainLicenseReminder> log)
        {
            _scopes = scopes;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try { await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); } catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try { await RemindAsync(stoppingToken); }
                catch (Exception ex) { _log.LogError(ex, "Captain license reminders failed"); }

                try { await Task.Delay(Every, stoppingToken); } catch (OperationCanceledException) { return; }
            }
        }

        private async Task RemindAsync(CancellationToken ct)
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var horizon = DateTime.UtcNow.Date.AddDays(30);
            var captains = await db.Users
                .Where(u => u.UserType == UserType.Delivery && !u.IsFrozen
                            && u.DrivingLicenseExpiry != null && u.DrivingLicenseExpiry <= horizon && u.LicenseReminderStage < 3)
                .ToListAsync(ct);

            foreach (var c in captains)
            {
                var days = (int)Math.Ceiling((c.DrivingLicenseExpiry!.Value.Date - DateTime.UtcNow.Date).TotalDays);
                int stage = days <= 0 ? 3 : days <= 7 ? 2 : 1;
                if (stage <= c.LicenseReminderStage) continue;

                var note = days <= 0
                    ? new SendNotification { UserId = c.Id, Title = "انتهت رخصة القيادة ⛔", Message = "انتهت رخصة القيادة. حدّث بياناتها وارفع نسخة جديدة." }
                    : new SendNotification { UserId = c.Id, Title = "تنبيه انتهاء الرخصة ⚠️", Message = $"تنتهي رخصة القيادة خلال {days} يوم. حدّث بياناتها من حسابك." };
                try { await notifications.SendNotificationAsync(note); } catch { /* best effort */ }
                c.LicenseReminderStage = stage;
            }
            await db.SaveChangesAsync(ct);
        }
    }
}
