using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class OrderTrackingStepDto
    {
        public string Title { get; set; } 
        public string Subtitle { get; set; } 
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
    }
}
