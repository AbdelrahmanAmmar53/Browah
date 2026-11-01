using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class ServiceOffer
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest { get; set; } = null!;

        public int ProviderId { get; set; }
        public User Provider { get; set; } = null!; 

        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ExecutionPeriod { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
