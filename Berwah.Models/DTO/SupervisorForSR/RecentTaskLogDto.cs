using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForSR
{
    public class RecentTaskLogDto
    {
        public string Title { get; set; } = string.Empty;
        public string ReviewText { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string ExecutionTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
