using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Delivery
{
    public class DeliveryOrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "ج.م";
        public string OrderType { get; set; } = string.Empty; 
        public string CategoryBadge { get; set; } = string.Empty; 

        // تفاصيل الاستلام (Pickup)
        public string PickupTitle { get; set; } = string.Empty; 
        public string PickupAddress { get; set; } = string.Empty; 
        public string PickupDistance { get; set; } = string.Empty; 

        // تفاصيل التوصيل (Dropoff)
        public string DropoffTitle { get; set; } = string.Empty; 
        public string DropoffAddress { get; set; } = string.Empty; 
        public string DropoffDistance { get; set; } = string.Empty; 

        // ملاحظات الدفع أو الاستلام
        public string PaymentNote { get; set; } = string.Empty; 
        public string ActionButtonText { get; set; } = string.Empty; 
    }
}
