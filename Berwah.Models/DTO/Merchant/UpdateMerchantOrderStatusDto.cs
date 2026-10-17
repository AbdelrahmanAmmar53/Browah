using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Merchant
{
    public class UpdateMerchantOrderStatusDto
    {
        public int OrderId { get; set; }
        public string NewStatus { get; set; }
    }
}
