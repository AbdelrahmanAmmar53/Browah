using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class AdvertisementResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsNegotiable { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PromotionPlan { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public List<SpecificationItemDto> Specifications { get; set; } = new();
    }
}
