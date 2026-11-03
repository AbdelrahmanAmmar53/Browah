using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public decimal WithdrawalFeeFixed { get; set; }
        public decimal CompanyCommissionPercentage { get; set; } = 10m;

        /// <summary>The captain's share of an order's total, in percent (0 or less = 15).</summary>
        public decimal CourierSharePercentage { get; set; } = 15m;

        public decimal MinWithdrawalAmount { get; set; } = 50m;
        public decimal MaxCashLimitForDelivery { get; set; }

    }
}
