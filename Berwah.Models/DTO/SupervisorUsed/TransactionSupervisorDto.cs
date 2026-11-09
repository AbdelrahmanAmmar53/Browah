using Berwah.Models.DTO.Delivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorUsed
{
    public class TransactionSupervisorDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; 
    }
}
