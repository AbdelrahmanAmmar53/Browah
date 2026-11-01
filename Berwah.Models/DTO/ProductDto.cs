using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public record ProductDto(int Id, string Name, string Description, decimal Price, decimal OriginalPrice, bool IsAvailableForInstantOrder, bool IsSpecialOffer, bool IsActive, string Currency, string ImageUrl, int Stock);
    //public class ProductDto
    //{
    //        public int Id { get; set; }
    //        public string Name { get; set; }
    //        public string Description { get; set; }
    //        public decimal Price { get; set; }
    //        public string Currency { get; set; } = "EGP";
    //        public string ImageUrl { get; set; }
    //        public IFormFile FormFile { get; set; }
    //        public int Stock { get; set; }
    //}
}
