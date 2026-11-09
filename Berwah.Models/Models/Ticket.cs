
using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        //public string TicketNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TicketStatus Status { get; set; }
        public TicketFor For { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public int? AdvertisementId { get; set; }
        public Advertisment Advertisement { get; set; }
    }
}
