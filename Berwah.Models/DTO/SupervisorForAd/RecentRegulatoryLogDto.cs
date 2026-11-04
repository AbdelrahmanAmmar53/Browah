using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForAd
{
    public class RecentRegulatoryLogDto
    {
        public int AdId { get; set; }
        public string AdNumber { get; set; }
        public string Title { get; set; }
        public string ActionType { get; set; }
        public string Reason { get; set; }
        public string TimeAgo { get; set; }
    }
}
