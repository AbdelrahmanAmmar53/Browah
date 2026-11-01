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
        public IFormFile? Attachment { get; set; }
    }
}
