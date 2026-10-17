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
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
