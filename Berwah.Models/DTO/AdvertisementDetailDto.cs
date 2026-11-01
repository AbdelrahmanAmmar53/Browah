using System;
using System.Collections.Generic;

namespace Berwah.Models.DTO
{
    public class AdvertisementDetailDto: AdvertisementCardDto
    {
       
        public bool IsNegotiable { get; set; }
        public string Condition { get; set; } = string.Empty;
        
        // الفئة
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;

        // معلومات البائع
        public int UserId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string SellerPhone { get; set; } = string.Empty;
        public string SellerImage { get; set; } = string.Empty;
        public bool IsSellerVerified { get; set; }

        
        // المواصفات
        public List<SpecificationItemDto> Specifications { get; set; } = new();
    }
}