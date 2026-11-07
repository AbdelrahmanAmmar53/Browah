using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForAd
{
    public class WithdrawalRequestDtoAdmin
    {
        public decimal Amount { get; set; }
        public string DestinationType { get; set; }
        public string AccountIdentifier { get; set; }
    }
}
