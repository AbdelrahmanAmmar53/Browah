namespace Berwah.Models.DTO
{
    public class AdvertisementReviewDto
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewerImage { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsVerifiedPurchase { get; set; }
    }
}