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
        public float TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string ConditionStaus { get; set; } = "New";
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
