using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class SupervisorAdStatusDto
    {
        public int PendingCount { get; set; }     // الإعلانات بانتظار المراجعة
        public int ApprovedCount { get; set; }    // الإعلانات المقبولة/المعتمدة
        public int RejectedCount { get; set; }    // الإعلانات المرفوضة (اختياري)
        public int SuspendedCount { get; set; }   // الإعلانات المعلقة (اختياري)
    }
}
