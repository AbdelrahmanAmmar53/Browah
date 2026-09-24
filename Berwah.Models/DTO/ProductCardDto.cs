namespace Berwah.Models.DTO
{
    public class ProductCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }

        public string Currency { get; set; } = "EGP";
        public string Location { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public string MainCategoryName { get; set; } = string.Empty;

        public string Condition { get; set; } = string.Empty; 
        public int UserId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public bool IsSellerVerified { get; set; }

        public bool Stock { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}