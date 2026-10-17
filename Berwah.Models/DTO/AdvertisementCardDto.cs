namespace Berwah.Models.DTO
{
    public class AdvertisementCardDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? ProductType { get; set; }
        public string City { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public string PromotionPlan { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}