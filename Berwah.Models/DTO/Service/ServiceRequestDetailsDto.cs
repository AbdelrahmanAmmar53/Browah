using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.Service
{
    public class ServiceRequestDetailsDto
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string SubCategoryName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientAddress { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public decimal EstimatedPrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
