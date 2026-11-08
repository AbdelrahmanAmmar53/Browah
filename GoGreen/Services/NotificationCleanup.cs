using Berwah.DAL.App;
using Microsoft.EntityFrameworkCore;

namespace Berwah.Services
{
    /// <summary>
    /// Deletes notifications older than <see cref="KeepDays"/> days, once a day, in small batches.
    /// Notifications carry no read flag, so one rule covers them all.
    /// </summary>
    public class NotificationCleanup : BackgroundService
    {
        /// <summary>How long a notification is kept.</summary>
        private const int KeepDays = 30;
        private const int BatchSize = 1000;
        private static readonly TimeSpan Every = TimeSpan.FromHours(24);

        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<NotificationCleanup> _log;

        public NotificationCleanup(IServiceScopeFactory scopes, ILogger<NotificationCleanup> log)
        {
            _scopes = scopes;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try { await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); } catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try { await CleanAsync(stoppingToken); }
                catch (Exception ex) { _log.LogError(ex, "Notification cleanup failed"); }

                try { await Task.Delay(Every, stoppingToken); } catch (OperationCanceledException) { return; }
            }
        }

        private async Task CleanAsync(CancellationToken ct)
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Notifications are stored in UTC or local time depending on who wrote them; a day's
            // difference does not matter against a 30 day window.
            var cutoff = DateTime.UtcNow.AddDays(-KeepDays);
            var removed = 0;
            while (!ct.IsCancellationRequested)
            {
                var old = await db.Notifications.Where(n => n.CreatedAt < cutoff).OrderBy(n => n.Id).Take(BatchSize).ToListAsync(ct);
                if (old.Count == 0) break;
                db.Notifications.RemoveRange(old);
                await db.SaveChangesAsync(ct);
                removed += old.Count;
            }
            if (removed > 0) _log.LogInformation("Deleted {Count} notifications older than {Days} days", removed, KeepDays);
        }
    }
}
