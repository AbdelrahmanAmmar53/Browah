namespace Berwah.Models.Models
{
    /// <summary>A customer's rating of one store on a delivered order, with the store's optional reply.</summary>
    public class StoreReview
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MerchantId { get; set; }
        public int CustomerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? Reply { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RepliedAt { get; set; }
    }
}
