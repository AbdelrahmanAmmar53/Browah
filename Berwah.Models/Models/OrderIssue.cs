namespace Berwah.Models.Models
{
    /// <summary>A problem a courier reported while delivering an order (customer unreachable, wrong address...).</summary>
    public class OrderIssue
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ReporterId { get; set; }
        /// <summary>customer_unreachable, wrong_address, store_closed, items_missing or other.</summary>
        public string Reason { get; set; } = "other";
        public string? Note { get; set; }
        /// <summary>Open until the admin deals with it.</summary>
        public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
