using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Delivery
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;     
        public string Subtitle { get; set; } = string.Empty;  
        public decimal Amount { get; set; }                   
        public string Type { get; set; } = string.Empty;     
        public string DateText { get; set; } = string.Empty;  
        public bool IsPositive { get; set; }
    }
}
