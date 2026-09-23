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
    }
}
