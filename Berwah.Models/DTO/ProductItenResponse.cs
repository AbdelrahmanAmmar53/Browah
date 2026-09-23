using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class ProductItemResponse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "EGP";
        public string ImageUrl { get; set; }
        public bool InStock { get; set; }
        public int StockQuantity { get; set; }
    }
}

