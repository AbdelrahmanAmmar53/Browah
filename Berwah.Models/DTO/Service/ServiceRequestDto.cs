using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Service
{
    public class ServiceRequestDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientImage { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? PickupAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? AttachmentUrl { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}
