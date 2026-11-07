using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForSR
{
    public class SupervisorAccountSettingsDto
    {
        public string FullName { get; set; } = string.Empty;
        public string RoleTitle { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public bool IsAvailableForFieldWork { get; set; }
        public double Rating { get; set; }
        public int CompletedTasksCount { get; set; }
        public double QualityKPIPercentage { get; set; }
        public double ResponseSpeedMinutes { get; set; }
        public bool PushNotificationsEnabled { get; set; }
    }
}
