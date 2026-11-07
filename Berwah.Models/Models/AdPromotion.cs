using Berwah.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Berwah.Models.Enum
{
    /// <summary>How the customer says they paid for a promotion plan.</summary>
    public enum AdPaymentMethod
    {
        Card = 1,
        EWallet = 2,
        InstaPay = 3
    }

    public enum AdPromotionStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}

namespace Berwah.Models.Models
{
    /// <summary>A request to promote an ad (Featured / VIP). It waits for an admin's approval; only then does the plan start.</summary>
    public class AdPromotionPurchase
    {
        public int Id { get; set; }

        public int AdvertismentId { get; set; }
        public Advertisment? Advertisment { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public PromotionPlanType Plan { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public AdPaymentMethod PaymentMethod { get; set; }

        /// <summary>What the customer typed for the payment: the card's last four digits, or the wallet / InstaPay number.</summary>
        [MaxLength(100)]
        public string? PaymentReference { get; set; }

        public AdPromotionStatus Status { get; set; } = AdPromotionStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }

    /// <summary>The numbers of the promotion plans, in one place.</summary>
    public static class AdPromotionRules
    {
        /// <summary>How long an ad is shown, counted from its approval.</summary>
        public const int AdLifetimeDays = 15;
        /// <summary>
        /// While the supervisors' accounts are being set up, a new ad is published at once instead of
        /// waiting for approval. Set to false to go back to supervisor approval.
        /// </summary>
        public static bool AutoApproveAds { get; } = true;

        /// <summary>The most VIP ads shown at the top of the home screen.</summary>
        public const int MaxVipOnHome = 5;

        public static decimal PriceOf(PromotionPlanType plan) => plan switch
        {
            PromotionPlanType.Featured => 50m,
            PromotionPlanType.VIP => 100m,
            _ => 0m
        };

        /// <summary>How many days the paid plan lasts; after it the ad is a normal one for the rest of its 15 days.</summary>
        public static int DaysOf(PromotionPlanType plan) => plan switch
        {
            PromotionPlanType.Featured => 2,
            PromotionPlanType.VIP => 5,
            _ => 0
        };

        /// <summary>Starts a plan on an ad (called when an admin approves the purchase).</summary>
        public static void Activate(Advertisment ad, PromotionPlanType plan, DateTime now)
        {
            ad.PromotionPlan = plan;
            var ends = now.AddDays(DaysOf(plan));
            if (ad.ExpiresAt.HasValue && ends > ad.ExpiresAt.Value) ends = ad.ExpiresAt.Value;
            ad.PromotionEndsAt = ends;
        }
    }
}
