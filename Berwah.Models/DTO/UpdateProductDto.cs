using Berwah.Models.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public bool IsAvailableForInstantOrder { get; set; }
        public bool IsSpecialOffer { get; set; }

        public int Stock { get; set; }
        // Optional: items have no location of their own (the store's pin is used).
        public string? Location { get; set; }
 
        public ProductCondition Condition { get; set; } = ProductCondition.NotApplicable;
        public int MenuSectionId { get; set; }

        public IFormFile? ImageFile { get; set; }

    }
}
