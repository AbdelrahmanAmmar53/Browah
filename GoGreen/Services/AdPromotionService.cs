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
        private readonly WalletService _wallet;

        public AdPromotionService(ApplicationDbContext db, INotificationService notifications, WalletService wallet)
        {
            _db = db;
            _notifications = notifications;
            _wallet = wallet;
        }

        /// <summary>The message to answer with when the request is not acceptable; null when it is.</summary>
        public static string? Validate(PromotionPlanType plan, AdPaymentMethod? method, string? reference)
        {
            if (plan == PromotionPlanType.Free) return null;
            if (!Enum.IsDefined(typeof(PromotionPlanType), plan)) return "الباقة غير صالحة";
            if (method == null || !Enum.IsDefined(typeof(AdPaymentMethod), method.Value)) return "اختر طريقة الدفع";
            if (method == AdPaymentMethod.Wallet) return null; // nothing to type: it comes out of the wallet
            if (string.IsNullOrWhiteSpace(reference)) return "أدخل بيانات الدفع";
            return null;
        }

        /// <summary>The message to answer with when the wallet can't pay for the plan; null when it can (or isn't the method).</summary>
        public async Task<string?> WalletShortfallAsync(int userId, PromotionPlanType plan, AdPaymentMethod? method)
        {
            if (method != AdPaymentMethod.Wallet || plan == PromotionPlanType.Free) return null;
            var price = AdPromotionRules.PriceOf(plan);
            var balance = await _wallet.BalanceAsync(userId);
            return balance < price ? $"رصيد محفظتك ({balance:0.##} ج.م) لا يكفي لهذه الباقة ({price:0.##} ج.م)" : null;
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

            // Paid from the wallet: the money is taken now and the plan starts at once.
            if (method == AdPaymentMethod.Wallet)
            {
                var paid = await _wallet.TryChargeAsync(userId, purchase.Amount, $"باقة إعلان ({(plan == PromotionPlanType.VIP ? "VIP" : "مميز")})");
                if (!paid)
                {
                    purchase.Status = AdPromotionStatus.Rejected;
                    purchase.ReviewedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                    return purchase;
                }
                await ApproveAsync(purchase.Id);
                return purchase;
            }

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
            await NotifySupervisorsAsync("طلب باقة جديد", $"طلب باقة {(plan == PromotionPlanType.VIP ? "VIP" : "مميز")} بقيمة {purchase.Amount:0.##} ج.م بانتظار المراجعة.");
            return purchase;
        }

        /// <summary>Tells every ads supervisor that something waits for them.</summary>
        public async Task NotifySupervisorsAsync(string title, string message, string? type = null, int? refId = null)
        {
            try
            {
                var ids = await _db.Users
                    .Where(u => u.UserRole.Any(ur => ur.Role.Name == "SupervisorForAd"))
                    .Select(u => u.Id)
                    .ToListAsync();
                foreach (var id in ids)
                    await _notifications.SendNotificationAsync(new SendNotification { UserId = id, Title = title, Message = message, Type = type, RefId = refId });
            }
            catch { }
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

            var shortfall = await WalletShortfallAsync(userId, plan, method);
            if (shortfall != null) return (shortfall, null);

            var purchase = await CreatePurchaseAsync(adId, userId, plan, method!.Value, reference);
            if (method == AdPaymentMethod.Wallet && purchase.Status == AdPromotionStatus.Rejected)
                return ("رصيد محفظتك لا يكفي لهذه الباقة", null);
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

        /// <summary>Every request of a status (null = all), newest first, with who asked and how they paid.</summary>
        public async Task<List<object>> ListAsync(AdPromotionStatus? status, int take = 200)
        {
            var query = _db.AdPromotionPurchases.AsNoTracking().AsQueryable();
            if (status.HasValue) query = query.Where(p => p.Status == status.Value);
            var rows = await query
                .OrderByDescending(p => p.CreatedAt)
                .Take(take)
                .Select(p => new
                {
                    p.Id,
                    p.AdvertismentId,
                    AdTitle = p.Advertisment!.Title,
                    p.UserId,
                    UserName = p.User != null ? (p.User.FirstName + " " + p.User.LastName) : "",
                    Plan = p.Plan.ToString(),
                    p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    p.PaymentReference,
                    Status = p.Status.ToString(),
                    p.CreatedAt,
                    p.ReviewedAt
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

        public async Task<bool> RejectAsync(int purchaseId, string? reason = null)
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
                    Message = string.IsNullOrWhiteSpace(reason)
                        ? "لم يتم اعتماد الدفع، وإعلانك يعمل كإعلان عادي. يمكنك طلب الباقة مرة أخرى."
                        : $"لم يتم اعتماد الباقة. السبب: {reason.Trim()}. إعلانك يعمل كإعلان عادي ويمكنك طلب الباقة مرة أخرى."
                });
            }
            catch { }
            return true;
        }
    }
}
