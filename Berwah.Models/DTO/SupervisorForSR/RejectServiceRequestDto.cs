using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForSR
{
    public class RejectServiceRequestDto
    {
        public string Reason { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
    }
}
