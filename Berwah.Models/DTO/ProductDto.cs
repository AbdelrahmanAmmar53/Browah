using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class ProductDto
    {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public string Currency { get; set; } = "EGP";
            public string ImageUrl { get; set; }
            public IFormFile FormFile { get; set; }
            public int Stock { get; set; }
    }
}
