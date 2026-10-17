using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Merchant
{
    public class MerchantProfileResponseDto
    {
        public int MerchantId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreDescription { get; set; } = string.Empty;
        public string BranchAddress { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = true; 

        // 2. حالة العمل
        public bool IsOpenForOrders { get; set; }
        public string WorkHours { get; set; } = "11:00 ص - 02:00 ص";

        // 3. مؤشرات الأداء
        public double Rating { get; set; } = 4.9;
        public int TotalReviewsCount { get; set; } = 1200;
        public string SupervisorName { get; set; } = string.Empty;
        public string AvgPreparationTime { get; set; } = "18 دقيقة";

        // 4. ملخص مبيعات اليوم
        public decimal TodaySales { get; set; }
        public int TodayOrdersCount { get; set; }
        public int ActiveProductsCount { get; set; } 
    }
}
