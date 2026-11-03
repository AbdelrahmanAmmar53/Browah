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
        public double? DropoffLatitude { get; set; }
        public double? DropoffLongitude { get; set; }

        // ملاحظات الدفع أو الاستلام
        public string PaymentNote { get; set; } = string.Empty; 
        public string ActionButtonText { get; set; } = string.Empty; 

        // Every store the order is picked up from (one when the cart had a single store).
        public List<DeliveryStopDto> Stops { get; set; } = new();
    }

    public class DeliveryStopDto
    {
        public int MerchantId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int ItemsCount { get; set; }
    }
}
