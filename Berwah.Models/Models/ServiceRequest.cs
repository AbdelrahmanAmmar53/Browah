using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }

        public string? PickupAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public DateTime? PreferredDate { get; set; }
        public string? PreferredTimeSlot { get; set; }
        public string Description { get; set; }
        public decimal EstimatedPrice { get; set; }
        public string? AttachmentUrl { get; set; }

        public ServiceRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
