using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForSR
{
    public class ReassignServiceRequestDto
    {
        public int NewProviderId { get; set; }
        public string ReassignmentReason { get; set; } = string.Empty; 
        public bool SendInstantNotification { get; set; } = true;
    }
}
