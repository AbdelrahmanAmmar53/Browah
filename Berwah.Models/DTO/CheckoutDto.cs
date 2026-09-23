using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class CheckoutDto
    {
        public int AddressId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public string? PromoCode { get; set; }
        public string PaymentMethod { get; set; } = "COD";
    }
}
