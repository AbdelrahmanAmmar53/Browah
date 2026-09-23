namespace Berwah.Models.DTO
{
    public class ProductCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public bool IsInStock { get; set; }
        public bool IsFavorite { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime CreeatedAt { get; set; }
    }
}