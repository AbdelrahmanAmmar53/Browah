using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForSR
{
    public class ApproveServiceRequestDto
    {
        public decimal FinalEstimatedPrice { get; set; }
        public int SubCategoryId { get; set; }
        public bool SendInstantNotification { get; set; } = true;
    }
}
