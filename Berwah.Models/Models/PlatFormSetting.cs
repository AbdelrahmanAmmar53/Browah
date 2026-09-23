using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class PlatFormSetting
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionValue { get; set; } 

        public bool IsPercentage { get; set; } 

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
