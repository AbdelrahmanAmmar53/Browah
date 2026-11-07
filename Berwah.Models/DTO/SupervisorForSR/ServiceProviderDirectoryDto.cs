using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForSR
{
    public class ServiceProviderDirectoryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public double Rating { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAvailable { get; set; }
        public string Location { get; set; } = string.Empty;
        public int CompletedTasksCount { get; set; }
    }
}
