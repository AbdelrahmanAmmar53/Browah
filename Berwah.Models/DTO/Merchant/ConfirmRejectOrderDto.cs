using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Merchant
{
    public class ConfirmRejectOrderDto
    {
        public string RejectionReason { get; set; } = string.Empty; 
        public bool DisableUnavailableItemsImmediately { get; set; } = true;
    }
}
