using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Service
{
    public class CreateServiceRequest
    {
        public int SubCategoryId { get; set; }
        public string? PickupAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public DateTime? PreferredDate { get; set; }
        public string? PreferredTimeSlot { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedPrice { get; set; }
        public IFormFile? Attachment { get; set; }
    }
}
