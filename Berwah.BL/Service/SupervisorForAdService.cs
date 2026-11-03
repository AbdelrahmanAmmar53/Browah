using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class SupervisorForAdService : ISupervisorForAdService
    {
        private readonly ISupervisorForAdRepository _supervisorRepo;
        private readonly INotificationService _notifications;

        public SupervisorForAdService(ISupervisorForAdRepository     supervisorRepo, INotificationService notifications)
        {
            _notifications = notifications;
            _supervisorRepo = supervisorRepo;
        }

        public async Task<IEnumerable<AdDetailsResponseDto>> GetAdsByStatusAsync(AdStatus status, int page, int pageSize)
        {
            var ads = await _supervisorRepo.GetAdsByStatusAsync(status, page, pageSize);

            return ads.Select(a => new AdDetailsResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Price = a.Price,
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt,
                SellerName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "مستخدم",
                ImageUrls = a.AdvertismentImages?.Select(ai => ai.Image?.Url ?? "").ToList() ?? new()
            });
        }

        public async Task<SupervisorAdStatusDto> GetAdsStatsAsync()
        {
            var pendingCount = await _supervisorRepo.GetAdsCountByStatusAsync(AdStatus.Pending);
            var approvedCount = await _supervisorRepo.GetAdsCountByStatusAsync(AdStatus.Approved);

            return new SupervisorAdStatusDto
            {
                PendingCount = pendingCount,
                ApprovedCount = approvedCount
            };
        }

        public async Task<bool> UpdateAdStatusAsync(int adId, AdStatus status)
        {
            var ad = await _supervisorRepo.GetByIdAsync(adId);
            if (ad == null) return false;

            ad.Status = status;
            var ok = await _supervisorRepo.UpdateAsync(ad);
            if (ok)
            {
                var (title, msg) = status switch
                {
                    AdStatus.Approved => ("تم اعتماد إعلانك ✅", $"إعلانك «{ad.Title}» أصبح منشورًا الآن."),
                    AdStatus.Rejected => ("تم رفض إعلانك", $"للأسف تم رفض إعلانك «{ad.Title}». يمكنك تعديله وإعادة نشره."),
                    AdStatus.Suspended => ("تم تجميد إعلانك", $"تم تجميد إعلانك «{ad.Title}» مؤقتًا."),
                    _ => ("", "")
                };
                if (title.Length > 0)
                    try { await _notifications.SendNotificationAsync(new SendNotification { UserId = ad.UserId, Title = title, Message = msg }); } catch { }
            }
            return ok;
        }
    }
}

