using Berwah.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Berwah.Models.Models
{
    public class Advertisment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public AdStatus Status { get; set; } = AdStatus.Pending;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsNegotiable { get; set; }
        public ProductCondition Condition { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Governorate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public PromotionPlanType PromotionPlan { get; set; }

        /// <summary>When the paid plan (Featured / VIP) stops. Null until an admin approves the purchase; after this the ad is a normal one.</summary>
        public DateTime? PromotionEndsAt { get; set; }

        /// <summary>When the ad stops being shown (15 days after its approval). Null for ads approved before this existed: they don't expire.</summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>The plan the ad is really running on right now: a paid plan only while it is active, otherwise Free.</summary>
        [NotMapped]
        public PromotionPlanType EffectivePlan =>
            PromotionPlan != PromotionPlanType.Free && PromotionEndsAt.HasValue && PromotionEndsAt.Value > DateTime.UtcNow
                ? PromotionPlan
                : PromotionPlanType.Free;

        public int UserId { get; set; }
        public User? User { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int SubCategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }

        public bool IsActive { get; set; }

        /// <summary>The supervisor's reason for the last decision (rejection / suspension reason). Cleared when approved.</summary>
        [MaxLength(500)]
        public string? ReviewNote { get; set; }

        /// <summary>When the supervisor last acted on the ad.</summary>
        public DateTime? ReviewedAt { get; set; }

        // Where the seller picked the ad's location on the map.
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // public int? SubCategoryId { get; set; }
        // public SubCategory? SubCategory { get; set; }

        public ICollection<AdvertismentImage> AdvertismentImages { get; set; } = new List<AdvertismentImage>();
        public ICollection<AdvertisementSpecification> Specifications { get; set; } = new List<AdvertisementSpecification>();
        public ICollection<AdvertisementReview> Reviews { get; set; } = new List<AdvertisementReview>();
    }
}