using System;
using System.ComponentModel.DataAnnotations;

namespace Berwah.Models.Models
{
    public class AdvertisementReview
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public Advertisment? Advertisement { get; set; }
        
        public int ReviewerId { get; set; }
        public User? Reviewer { get; set; }
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsVerifiedPurchase { get; set; }
    }
}