using Berwah.Models.DTO.Delivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorUsed
{
    public class SupervisorWalletDto
    {
        public decimal CurrentBalance { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal TotalEarnings { get; set; }
        public List<TransactionSupervisorDto> RecentTransactions { get; set; } = new();
    }
}
