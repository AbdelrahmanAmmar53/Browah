using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class TicketSummaryDyo
    {
        public string TicketNumber { get; set; }
        public int TotalTickets { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string For { get; set; }
        public int ClosedTickets { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
