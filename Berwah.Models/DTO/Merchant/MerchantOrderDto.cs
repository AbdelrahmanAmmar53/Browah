namespace Berwah.Models.DTO.Merchant
{
    public class MerchantOrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty; 
        public string StatusBadgeText { get; set; } = string.Empty;
        public List<MerchantOrderItemDto> Items { get; set; } = new();
        public string PaymentMethod { get; set; } = string.Empty;
    }
}