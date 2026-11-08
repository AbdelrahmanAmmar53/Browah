using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.DTO.SupervisorUsed;
using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class SupervisorUsedService : ISupervisorUsedService
    {
        private readonly ISupervisorUsedRepository _repo;
        private readonly INotificationService _notifications; 

        public SupervisorUsedService(ISupervisorUsedRepository repo, INotificationService notifications)
        {
            _repo = repo;
            _notifications = notifications;
        }

        public async Task<UsedAdStatusDto> GetUsedAdsStatsAsync()
        {
            return await _repo.GetStatsAsync();
        }

        public async Task<IEnumerable<object>> GetUsedAdsByStatusAsync(AdStatus? status, int page, int pageSize)
        {
            var ads = await _repo.GetUsedAdsByStatusAsync(status, page, pageSize);

            return ads.Select(a => new
            {
                a.Id,
                Title = a.Title ?? "بدون عنوان",
                Price = a.Price,
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt,
                PublisherName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}".Trim() : "مستخدم مجهول",
                ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url ?? ""
            });
        }

        public async Task<object?> GetUsedAdDetailsAsync(int id)
        {
            var ad = await _repo.GetAdByIdAsync(id);
            if (ad == null) return null;

            return new
            {
                ad.Id,
                ad.Title,
                ad.Description,
                ad.Price,
                Status = ad.Status.ToString(),
                Publisher = ad.User != null ? new
                {
                    ad.User.Id,
                    FullName = $"{ad.User.FirstName} {ad.User.LastName}".Trim(),
                    ad.User.PhoneNumber,
                    ad.User.IsVerified
                } : null,
                Images = ad.AdvertismentImages?.Select(ai => new
                {
                    ai.Image?.Url,
                    IsMain = ai.IsMain
                })
            };
        }

        public async Task<bool> ApproveAndPublishAdAsync(int id, UsedAdActionDto dto)
        {
            var ad = await _repo.GetAdByIdAsync(id);
            if (ad == null) return false;

            ad.Status = AdStatus.Approved;
            if (dto.ApprovedPrice.HasValue) ad.Price = dto.ApprovedPrice.Value;
            if (!string.IsNullOrEmpty(dto.ApprovedDescription)) ad.Description = dto.ApprovedDescription;

            var updated = await _repo.UpdateAdAsync(ad);

            if (updated && dto.SendNotification && ad.UserId>0)
            {
                try
                {
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = ad.UserId,
                        Title = "تم قبول إعلانك المستعمل 🎉",
                        Message = $"تم اعتماد ونشر إعلانك #{ad.Id} بنجاح."
                    });
                }
                catch { }
            }

            return updated;
        }

        public async Task<bool> RequestEditFromPublisherAsync(int id, string? reason)
        {
            var ad = await _repo.GetAdByIdAsync(id);
            if (ad == null) return false;

            ad.Status = AdStatus.Pending; 
            var updated = await _repo.UpdateAdAsync(ad);

            if (updated && ad.UserId>0)
            {
                try
                {
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = ad.UserId,
                        Title = "طلب تعديل على إعلانك 📝",
                        Message = $"يرجى تعديل الإعلان #{ad.Id}. السبب: {reason ?? "راجع التفاصيل"}"
                    });
                }
                catch { }
            }

            return updated;
        }

        public async Task<bool> SuspendAdAsync(int id, string? reason)
        {
            var ad = await _repo.GetAdByIdAsync(id);
            if (ad == null) return false;

            ad.Status = AdStatus.Suspended;
            return await _repo.UpdateAdAsync(ad);
        }

        public async Task<bool> RejectAdWithPenaltyAsync(int id, UsedAdActionDto dto)
        {
            var ad = await _repo.GetAdByIdAsync(id);
            if (ad == null) return false;

            ad.Status = AdStatus.Rejected;
            var updated = await _repo.UpdateAdAsync(ad);

            if (updated && dto.SendNotification && ad.UserId>0)
            {
                try
                {
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = ad.UserId,
                        Title = "تم رفض الإعلان ❌",
                        Message = $"عذراً، تم رفض إعلانك المستعمل #{ad.Id}. السبب: {dto.Reason ?? "مخالفة الشروط"}"
                    });
                }
                catch { }
            }

            return updated;
        }
    }
}
    
