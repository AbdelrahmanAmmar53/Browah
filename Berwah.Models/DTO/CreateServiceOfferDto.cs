using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class CreateServiceOfferDto
    {
        public int ServiceRequestId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ExecutionPeriod { get; set; } = string.Empty;
        // Where the provider is, picked on the map by the app (optional).
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public IFormFile? Attachment { get; set; }
    }
}
