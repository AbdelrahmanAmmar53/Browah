using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Merchant
{
    public class MerchantDashboardResponseDto
    {
        public string StoreName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public bool IsOpenForOrders { get; set; }

        public int TotalActiveOrders { get; set; }
        public int CookingOrAssemblyOrders { get; set; }
        public int ReadyForDeliveryOrders { get; set; }
        public decimal TodaySales { get; set; }

        public List<MerchantOrderDto> Orders { get; set; } = new();
    }
}
