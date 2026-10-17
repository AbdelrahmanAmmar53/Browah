using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class UpdateTransactionStatusDto
    {
        public int TargetUserId { get; set; }
        public string NewStatus { get; set; }
    }
}
