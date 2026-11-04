using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForAd
{
    public class RegulatoryActionDto
    {
        public AdStatus Status { get; set; }
        public string SupervisorNotes { get; set; }
        public bool SendInstantNotification { get; set; }
    }
}
