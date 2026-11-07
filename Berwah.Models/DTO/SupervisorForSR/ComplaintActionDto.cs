using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForSR
{
    public class ComplaintActionDto
    {
        public string SupervisorNote { get; set; } = string.Empty;
        public bool SendNotification { get; set; }
    }
}
