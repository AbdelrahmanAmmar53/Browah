using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class LiveTrackingDto
    {
        public int OrderId { get; set; }
        public string RemainingTime { get; set; } = string.Empty;
        public double DeliveryLatitude { get; set; }
        public double DeliveryLongitude { get; set; }
        public double UserLatitude { get; set; }
        public double UserLongitude { get; set; }
        public string DeliveryManName { get; set; } = string.Empty;
        public double DeliveryManRating { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;

        /// <summary>Pending, Preparing, PickedUp, OnTheWay, Delivered or Cancelled.</summary>
        public string Status { get; set; } = "Pending";
        public string DeliveryManPhone { get; set; } = string.Empty;
        public List<LiveTrackingItemDto> Items { get; set; } = new();
        /// <summary>Money returning to the customer for cancelled items (online payments only).</summary>
        public decimal RefundAmount { get; set; }
    }

    public class LiveTrackingItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public bool IsCancelled { get; set; }
    }
}
