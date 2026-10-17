using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class UserTransactionDto
    {
        public int OrderId { get; set; }
        public string Title { get; set; }
        public string TransactionDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } 
    }
}
