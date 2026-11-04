using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForAd
{
    public class AdReportResponse
    {
        public int AdsCount { get; set; }
        public double ApprovalRate { get; set; }
        public double RejectionRate { get; set; }
        public double AverageCheckSpeedMinutes { get; set; } 
        public List<CategoryDistributionDto> CategoryDistributions { get; set; } 
        public List<RecentRegulatoryLogDto> RecentLogs { get; set; } = new();
    }
}
