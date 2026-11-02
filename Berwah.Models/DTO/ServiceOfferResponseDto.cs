using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class ServiceOfferResponseDto
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public int ProviderId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ExecutionPeriod { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
