using System.Collections.Generic;

namespace Berwah.Models.DTO.Merchant
{
    /// <summary>A store's page for the customer: the store itself and its menu.</summary>
    public class StoreMenuDto
    {
        public int MerchantId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessDescription { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string? MerchantImageUrl { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
        public string? WorkHours { get; set; }
        public decimal? Rating { get; set; }
        public int? ReviewsCount { get; set; }
        public List<StoreMenuSectionDto> Sections { get; set; } = new();
    }

    public class StoreMenuSectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public List<ProductCardDto> Items { get; set; } = new();
    }
}
