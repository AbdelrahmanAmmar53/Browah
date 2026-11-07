using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForSR
{
    public class SupervisorUpdateServiceRequestDto
    {
        public decimal EstimatedPrice { get; set; }
        public int SubCategoryId { get; set; }
        public string SupervisorNotes { get; set; } = string.Empty;
    }
}
