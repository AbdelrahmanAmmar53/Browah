using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Delivery
{
    public class WithdrawalRequestDto
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string DestinationAccount { get; set; }
    }
}
