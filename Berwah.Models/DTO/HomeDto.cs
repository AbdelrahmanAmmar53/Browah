using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class HomeDto
    {
        public List<AdvertisementCardDto> Banners { get; set; } = new();
        public List<CategorySummaryDto> Categories { get; set; } = new();
        public List<AdvertisementCardDto> Advertisements { get; set; } = new();
        public List<AdvertisementCardDto> RecentlyViewedProducts { get; set; } = new();
        public List<AdvertisementCardDto> LatestProducts { get; set; } = new();
    }
}
