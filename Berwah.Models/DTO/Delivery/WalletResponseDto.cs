using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Delivery
{
     public class WalletResponseDto
    {
        public decimal TodayEarnings { get; set; }
        public decimal WeeklyEarnings { get; set; }
        public decimal MonthlyEarnings { get; set; }
        public int CompletedOrdersCount { get; set; }
        public string DrivingHours { get; set; } = "6.5 ساعة";
        public decimal AvgOrderEarnings { get; set; }

        public decimal CashInHand { get; set; }       // الكاش الموجود مع الكابتن ويجب توريده
        public decimal DigitalBalance { get; set; }   // الأرباح الرقمية القابلة للسحب

        public List<TransactionDto> RecentTransactions { get; set; } = new();
    }
}
