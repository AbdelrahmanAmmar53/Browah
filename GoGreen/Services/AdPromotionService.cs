using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace Berwah.Services
{
    /// <summary>Records a customer's request for a Featured / VIP plan and lets an admin approve or reject it.</summary>
    public class AdPromotionService
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notifications;

        public AdPromotionService(ApplicationDbContext db, INotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        /// <summary>The message to answer with when the request is not acceptable; null when it is.</summary>
        public static string? Validate(PromotionPlanType plan, AdPaymentMethod? method, string? reference)
        {
            if (plan == PromotionPlanType.Free) return null;
            if (!Enum.IsDefined(typeof(PromotionPlanType), plan)) return "الباقة غير صالحة";
            if (method == null || !Enum.IsDefined(typeof(AdPaymentMethod), method.Value)) return "اختر طريقة الدفع";
            if (string.IsNullOrWhiteSpace(reference)) return "أدخل بيانات الدفع";
            return null;
        }

        public async Task<AdPromotionPurchase> CreatePurchaseAsync(int adId, int userId, PromotionPlanType plan, AdPaymentMethod method, string? reference)
        {
            var purchase = new AdPromotionPurchase
            {
                AdvertismentId = adId,
                UserId = userId,
                Plan = plan,
                Amount = AdPromotionRules.PriceOf(plan),
                PaymentMethod = method,
                PaymentReference = reference?.Trim(),
                Status = AdPromotionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _db.AdPromotionPurchases.Add(purchase);
            await _db.SaveChangesAsync();

            try
            {
                await _notifications.SendNotificationAsync(new SendNotification
                {
                    UserId = userId,
                    Title = "تم استلام طلب الباقة",
                    Message = "طلب الباقة قيد المراجعة، وستبدأ الباقة فور اعتماد الدفع."
                });
            }
            catch { }
            return purchase;
        }

        /// <summary>Asks for a plan on an ad that already exists. Returns an error message, or null with the purchase.</summary>
        public async Task<(string? Error, AdPromotionPurchase? Purchase)> RequestForExistingAdAsync(
            int adId, int userId, PromotionPlanType plan, AdPaymentMethod? method, string? reference)
        {
            if (plan == PromotionPlanType.Free) return ("اختر باقة مميز أو VIP", null);
            var invalid = Validate(plan, method, reference);
            if (invalid != null) return (invalid, null);

            var ad = await _db.Advertisements.FirstOrDefaultAsync(a => a.Id == adId && a.UserId == userId);
            if (ad == null) return ("الإعلان غير موجود", null);
            if (ad.Status == AdStatus.Rejected || ad.Status == AdStatus.Suspended) return ("لا يمكن ترقية هذا الإعلان", null);
            if (ad.ExpiresAt.HasValue && ad.ExpiresAt.Value <= DateTime.UtcNow) return ("انتهت مدة الإعلان", null);
            if (ad.EffectivePlan != PromotionPlanType.Free) return ("الإعلان عليه باقة سارية بالفعل", null);
            if (await _db.AdPromotionPurchases.AnyAsync(p => p.AdvertismentId == adId && p.Status == AdPromotionStatus.Pending))
                return ("يوجد طلب باقة قيد المراجعة لهذا الإعلان", null);

            var purchase = await CreatePurchaseAsync(adId, userId, plan, method!.Value, reference);
            return (null, purchase);
        }

        public async Task<List<object>> PendingAsync()
        {
            var rows = await _db.AdPromotionPurchases
                .AsNoTracking()
                .Where(p => p.Status == AdPromotionStatus.Pending)
                .OrderBy(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.AdvertismentId,
                    AdTitle = p.Advertisment!.Title,
                    p.UserId,
                    Plan = p.Plan.ToString(),
                    p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    p.PaymentReference,
                    p.CreatedAt
                })
                .ToListAsync();
            return rows.Cast<object>().ToList();
        }

        public async Task<bool> ApproveAsync(int purchaseId)
        {
            var purchase = await _db.AdPromotionPurchases.Include(p => p.Advertisment).FirstOrDefaultAsync(p => p.Id == purchaseId);
            if (purchase == null || purchase.Status != AdPromotionStatus.Pending || purchase.Advertisment == null) return false;

            var now = DateTime.UtcNow;
            AdPromotionRules.Activate(purchase.Advertisment, purchase.Plan, now);
            purchase.Status = AdPromotionStatus.Approved;
            purchase.ReviewedAt = now;
            await _db.SaveChangesAsync();

            try
            {
                await _notifications.SendNotificationAsync(new SendNotification
                {
                    UserId = purchase.UserId,
                    Title = "تم تفعيل الباقة ✅",
                    Message = $"بدأت باقة إعلانك «{purchase.Advertisment.Title}» ."
                });
            }
            catch { }
            return true;
        }

        public async Task<bool> RejectAsync(int purchaseId)
        {
            var purchase = await _db.AdPromotionPurchases.Include(p => p.Advertisment).FirstOrDefaultAsync(p => p.Id == purchaseId);
            if (purchase == null || purchase.Status != AdPromotionStatus.Pending) return false;

            purchase.Status = AdPromotionStatus.Rejected;
            purchase.ReviewedAt = DateTime.UtcNow;
            // The ad stays as a normal one.
            if (purchase.Advertisment != null && purchase.Advertisment.PromotionEndsAt == null)
                purchase.Advertisment.PromotionPlan = PromotionPlanType.Free;
            await _db.SaveChangesAsync();

            try
            {
                await _notifications.SendNotificationAsync(new SendNotification
                {
                    UserId = purchase.UserId,
                    Title = "تعذّر تفعيل الباقة",
                    Message = "لم يتم اعتماد الدفع، وإعلانك يعمل كإعلان عادي. يمكنك طلب الباقة مرة أخرى."
                });
            }
            catch { }
            return true;
        }
    }
}
