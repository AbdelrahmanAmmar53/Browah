using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorUsed
{
    public class SupervisorProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string RoleTitle { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int CompletedTasksCount { get; set; }
        public double QualityKPIPercentage { get; set; }
        public double ResponseSpeedMinutes { get; set; }
    }
}
