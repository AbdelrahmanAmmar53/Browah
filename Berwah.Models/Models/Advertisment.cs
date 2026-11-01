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

        public int UserId { get; set; }
        public User? User { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int SubCategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }

        public bool IsActive { get; set; }

        // public int? SubCategoryId { get; set; }
        // public SubCategory? SubCategory { get; set; }

        public ICollection<AdvertismentImage> AdvertismentImages { get; set; } = new List<AdvertismentImage>();
        public ICollection<AdvertisementSpecification> Specifications { get; set; } = new List<AdvertisementSpecification>();
        public ICollection<AdvertisementReview> Reviews { get; set; } = new List<AdvertisementReview>();
    }
}