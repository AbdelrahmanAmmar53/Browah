using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForSR
{
    public class ComplaintStatsDto
    {
        public int UrgentCount { get; set; }
        public double ResolutionSpeedMinutes { get; set; }
        public double ComplianceRate { get; set; }
    }
}
