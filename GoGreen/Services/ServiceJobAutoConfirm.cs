using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace Berwah.Services
{
    /// <summary>
    /// A customer who never confirms a finished job should not keep the provider waiting:
    /// two days after the provider said "done", the job is confirmed on its own (without a rating).
    /// </summary>
    public class ServiceJobAutoConfirm : BackgroundService
    {
        private static readonly TimeSpan Grace = TimeSpan.FromHours(48);
        private static readonly TimeSpan Every = TimeSpan.FromMinutes(30);

        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<ServiceJobAutoConfirm> _log;

        public ServiceJobAutoConfirm(IServiceScopeFactory scopes, ILogger<ServiceJobAutoConfirm> log)
        {
            _scopes = scopes;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Let the app finish starting before the first look.
            try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); } catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConfirmDueJobsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Auto-confirming finished service jobs failed");
                }

                try { await Task.Delay(Every, stoppingToken); } catch (OperationCanceledException) { return; }
            }
        }

        private async Task ConfirmDueJobsAsync(CancellationToken ct)
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var limit = DateTime.UtcNow - Grace;
            var due = await db.ServiceRequests
                .Where(r => r.Status == ServiceRequestStatus.InProgress && r.ProviderDoneAt != null && r.ProviderDoneAt < limit)
                .ToListAsync(ct);
            if (due.Count == 0) return;

            foreach (var request in due)
            {
                request.Status = ServiceRequestStatus.Completed;
                request.CompletedAt = DateTime.UtcNow;
            }
            await db.SaveChangesAsync(ct);

            foreach (var request in due)
            {
                var providerId = await db.ServiceOffers.Where(o => o.Id == request.AcceptedOfferId)
                    .Select(o => (int?)o.ProviderId).FirstOrDefaultAsync(ct);

                await notifications.SendNotificationAsync(new SendNotification
                {
                    UserId = request.UserId,
                    Title = "تم تأكيد طلبك تلقائيًا",
                    Message = $"مرّت 48 ساعة على إنهاء الطلب #{request.Id}، فتم اعتباره مكتملًا."
                });
                if (providerId != null)
                {
                    await notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = providerId.Value,
                        Title = "تم تأكيد المهمة تلقائيًا",
                        Message = $"اكتمل الطلب #{request.Id} بعد مرور 48 ساعة على إنهائك له."
                    });
                }
            }
        }
    }
}
