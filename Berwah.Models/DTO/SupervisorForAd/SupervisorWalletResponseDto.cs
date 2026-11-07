using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForAd
{
    public class SupervisorWalletResponseDto
    {
        public decimal Balance { get; set; }
        public decimal TotalDues { get; set; }
        public decimal PendingAuditBalance { get; set; }
        public double CompletionRate { get; set; }
        public List<WalletTransactionDto> RecentTransactions { get; set; }
    }
}
