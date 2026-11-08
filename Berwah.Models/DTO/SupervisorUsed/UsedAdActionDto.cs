using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorUsed
{
    public class UsedAdActionDto
    {
        public string? Reason { get; set; }
        public decimal? ApprovedPrice { get; set; }
        public string? ApprovedDescription { get; set; }
        public bool SendNotification { get; set; }

    }
}
