using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class UserProfileDto
    {
       // public string UserId { get; set; }
        public string FullName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string JoinedDate { get; set; } 
        public double Rating { get; set; } 
        public int ActiveAdsCount { get; set; } 
        public int TodayViewsCount { get; set; }
        public List<AdSummaryDto> LatestAds { get; set; } = new();
    }
}
