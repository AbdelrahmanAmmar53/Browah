using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class OrderSummeryDto
    {
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        /// <summary>same_city, other_city, blocked_governorate, blocked_country or unknown.</summary>
        public string DeliveryZone { get; set; } = "unknown";
        /// <summary>Km from the farthest store of the order to the delivery spot (0 when unknown).</summary>
        public double DistanceKm { get; set; }
        /// <summary>The part of the delivery fee that is for the km beyond the normal distance.</summary>
        public decimal ExtraDeliveryFee { get; set; }
        public double FreeDistanceKm { get; set; }
        public decimal PricePerExtraKm { get; set; }
    }
}
