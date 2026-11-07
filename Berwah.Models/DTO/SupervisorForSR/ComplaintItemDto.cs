using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForSR
{
    public class ComplaintItemDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string RelatedRequestNumber { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // عاجل جداً، عادي
        public string Status { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientLocation { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string ServiceTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
    }
}
