using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class CheckoutDto
    {
        public string Address { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public string? PromoCode { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public double   UserLatitude { get; set; }
        public double UserLongitude { get; set; }
        public int? AdditionalServiceId { get; set; }
    }
}
