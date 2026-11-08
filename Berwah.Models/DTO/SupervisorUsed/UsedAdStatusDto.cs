using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorUsed
{
    public class UsedAdStatusDto
    {
        public int PendingCount { get; set; }
        public int ApprovedTodayCount { get; set; }
        public int WaitingCount { get; set; }
        public int TotalCount { get; set; }
    }
}
