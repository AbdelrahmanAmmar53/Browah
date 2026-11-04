using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.DTO.SupervisorForAd;
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
            var rejectedCount = await _supervisorRepo.GetAdsCountByStatusAsync(AdStatus.Rejected);
            var suspendedCount = await _supervisorRepo.GetAdsCountByStatusAsync(AdStatus.Suspended);

            return new SupervisorAdStatusDto
            {
                PendingCount = pendingCount,
                ApprovedCount = approvedCount,
                RejectedCount = rejectedCount,   
                SuspendedCount = suspendedCount   
            };
        }

        public async Task<bool> UpdateAdStatusAsync(int adId, AdStatus status)
        {
            var ad = await _supervisorRepo.GetAdDetailsByIdAsync(adId);
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

        // جلب تفاصيل الإعلان الكاملة لشاشة التعديل والمراجعة
        public async Task<AdDetailsResponseDto?> GetAdDetailsAsync(int id)
        {
            var a = await _supervisorRepo.GetAdDetailsByIdAsync(id);
            if (a == null) return null;

            return new AdDetailsResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Price = a.Price,
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt,
                SellerName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "مستخدم",
                ImageUrls = a.AdvertismentImages?.Select(ai => ai.Image?.Url ?? "").ToList() ?? new()
            };
        }

        // تعديل بيانات الإعلان من قبل المشرف
        public async Task<bool> UpdateAdDetailsAsync(int id, UpdateDetailAdDto dto)
        {
            var ad = await _supervisorRepo.GetAdDetailsByIdAsync(id);
            if (ad == null) return false;

            ad.Title = dto.Title;
            ad.Description = dto.Description;
            ad.Price = dto.Price;
            ad.CategoryId = dto.CategoryId;

            return await _supervisorRepo.UpdateAsync(ad);
        }

        // رفض الإعلان مع سبب الرفض وتوجيهات المشرف
        public async Task<bool> RejectAdWithReasonAsync(int id, RejectAdDto dto)
        {
            var ad = await _supervisorRepo.GetAdDetailsByIdAsync(id);
            if (ad == null) return false;

            ad.Status = AdStatus.Rejected;
            // يمكنك حفظ سبب الرفض في حقل مخصص إذا أضفته لجدول الـ Advertisement
            var updated = await _supervisorRepo.UpdateAsync(ad);

            if (updated && ad.UserId > 0)
            {
                try
                {
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = ad.UserId,
                        Title = "سبب رفض الإعلان",
                        Message = $"تم رفض إعلانك «{ad.Title}». السبب: {dto.RejectReason}. {dto.SupervisorNotes}"
                    });
                }
                catch { }
            }
            return updated;
        }

        // إخفاء / تعليق الإعلان وإرسال إشعار وSMS اختياري
        public async Task<bool> HideAdAsync(int id, HideAdDto dto)
        {
            var ad = await _supervisorRepo.GetAdDetailsByIdAsync(id);
            if (ad == null) return false;

            ad.Status = AdStatus.Suspended;
            var updated = await _supervisorRepo.UpdateAsync(ad);

            if (updated && ad.UserId > 0)
            {
                try
                {
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = ad.UserId,
                        Title = "تعليق الإعلان عن العرض",
                        Message = dto.NotificationMessage
                    });

                    // إذا كان مفعل إرسال SMS يمكنك تضمين خدمة الـ SMS هنا
                }
                catch { }
            }
            return updated;
        }
        public async Task<AdReportResponse> GetReportsAsync(string period)
        {
            DateTime? startDate = period.ToLower() switch
            {
                "today" => DateTime.UtcNow.Date,
                "week" => DateTime.UtcNow.AddDays(-7),
                "month" => DateTime.UtcNow.AddDays(-30),
                _ => null
            };

            var totalExamined = await _supervisorRepo.GetTotalExaminedCountAsync(startDate, null);
            var approvedCount = await _supervisorRepo.GetCountByStatusInRangeAsync(AdStatus.Approved, startDate, null);
            var rejectedCount = await _supervisorRepo.GetCountByStatusInRangeAsync(AdStatus.Rejected, startDate, null);

            double approvalRate = totalExamined > 0 ? Math.Round((double)approvedCount / totalExamined * 100, 1) : 0;
            double rejectionRate = totalExamined > 0 ? Math.Round((double)rejectedCount / totalExamined * 100, 1) : 0;

            // 🟢 جلب توزيع الأصناف من خلال الـ Repository (بدون استخدام _context نهائياً)
            var rawCategories = await _supervisorRepo.GetCategoryDistributionsAsync(startDate);

            int totalCatAds = rawCategories.Sum(c => c.AdsCount);

            var categoriesData = rawCategories.Select(c => new CategoryDistributionDto
            {
                CategoryName = c.CategoryName,
                AdsCount = c.AdsCount,
                // 🟢 حساب النسبة المئوية بدقة لكل صنف بناءً على إجمالي الإعلانات
                Presentage = totalCatAds > 0 ? Math.Round((double)c.AdsCount / totalCatAds * 100, 1) : 0
            }).ToList();

            // جلب أحدث الإجراءات المسجلة
            var recentLogsEntities = await _supervisorRepo.GetRecentLogsAsync(5);
            var recentLogs = recentLogsEntities.Select(a => new RecentRegulatoryLogDto
            {
                AdId = a.Id,
                AdNumber = $"#AD-{a.Id}",
                Title = a.Title,
                ActionType = a.Status == AdStatus.Approved ? "اعتماد إعلان" : (a.Status == AdStatus.Rejected ? "رفض إعلان" : "إخفاء إعلان"),
                Reason = a.Status == AdStatus.Rejected ? "مخالفة معايير المنتجات الجديدة" : "مطابقة مستوى دقة شروط الجديد",
                TimeAgo = $"{Math.Max(1, (DateTime.UtcNow - a.CreatedAt).Minutes)} دقائق"
            }).ToList();

            return new AdReportResponse
            {
                AdsCount = totalExamined,
                ApprovalRate = approvalRate,
                RejectionRate = rejectionRate,
                AverageCheckSpeedMinutes = 3.8,
                CategoryDistributions = categoriesData.OrderByDescending(c => c.AdsCount).ToList(),
                RecentLogs = recentLogs
            };
        }
        public async Task<bool> ExecuteRegulatoryActionAsync(int adId, RegulatoryActionDto dto)
        {
            var ad = await _supervisorRepo.GetAdDetailsByIdAsync(adId);
            if (ad == null) return false;

            ad.Status = dto.Status;
            var updated = await _supervisorRepo.UpdateAsync(ad);

            if (updated && dto.SendInstantNotification && ad.UserId > 0)
            {
                try
                {
                    var (title, msg) = dto.Status switch
                    {
                        AdStatus.Approved => ("تحديث الإجراء الرقابي ✅", $"تم تفعيل وإعادة نشر إعلانك «{ad.Title}» في السوق المفتوح."),
                        AdStatus.Rejected => ("رفض نهائي للإعلان ❌", $"تم رفض إعلانك نهائياً. ملاحظات المشرف: {dto.SupervisorNotes}"),
                        AdStatus.Suspended => ("تعليق مؤقت للإعلان ⚠️", $"تم حجب إعلانك مؤقتاً لعدم استيفاء شروط الرقابة. {dto.SupervisorNotes}"),
                        _ => ("تحديث إداري", "تم تحديث حالة إعلانك.")
                    };

                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = ad.UserId,
                        Title = title,
                        Message = msg
                    });
                }
                catch { }
            }

            return updated;
        }
    }
}

