using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Merchant
{
    public class RejectOrderDetailsDto
    {
        public int orderid { get; set; }
        public string OrderNumber { get; set; }
        public decimal TotalPrice { get; set; }
        public string CustomerName { get; set; }
        public string PaymentMethod { get; set; }
        public string CustomerAddress { get; set; }

        public decimal PenaltyAmount { get; set; } = 35.00m; 
        public string AcceptanceRateImpact { get; set; } = "-2.5%"; 
        public string AppearancePenaltyDuration { get; set; } = "4 ساعات"; 

        public List<MerchantOrderItemDto> Items { get; set; }

    }
}
