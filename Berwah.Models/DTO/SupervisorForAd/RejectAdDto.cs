using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForAd
{
    public class RejectAdDto
    {
        public string RejectReason { get; set; }
        public string? SupervisorNotes { get; set; }
        public bool AllowResubmission { get; set; }
    }
}
