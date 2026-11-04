using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public float TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? DeliveryId { get; set; }
        public User? Delivery { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string ConditionStaus { get; set; } = "New";
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        // --- إحداثيات موقع الاستلام (المطعم / البائع) ---
        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }

        // --- إحداثيات موقع التسليم (العميل) ---
        public double DropoffLatitude { get; set; }
        public double DropoffLongitude { get; set; }
        public string? DropoffAddress { get; set; }
        // The customer's rating of the courier after delivery (1-5).
        // When the order first showed on the couriers' radar, when a courier took it, when it was delivered (UTC).
        public DateTime? FirstOfferedAt { get; set; }
        public DateTime? CourierAcceptedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public int? CourierRating { get; set; }
        public string? CourierReview { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
