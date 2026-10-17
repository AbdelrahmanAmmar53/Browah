using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Delivery
{
    public class DepositeCashDto
    {
        public decimal Amount { get; set; }
        public string TransferMethod { get; set; }
        public string CashUrl { get; set; }
    }
}
