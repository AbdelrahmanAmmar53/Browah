using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Merchant
{
    public class MerchantCardDto
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessDescription { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string? MerchantImageUrl { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
    }
}
