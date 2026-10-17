using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class OrderConfirmationDto
    {
        public int OrderId { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string DeliveryTimeEstimate { get; set; } = string.Empty;

        public List<OrderTrackingStepDto> TrackingSteps { get; set; } = new();

        public DeliveryMapDto? DeliveryMan { get; set; }

        public string DeliveryAddress { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethodText { get; set; } = string.Empty;
    }
}
