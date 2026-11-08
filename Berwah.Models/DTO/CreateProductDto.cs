using Berwah.Models.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public record CreateProductDto(string Name,string Description,decimal Price,int MenuSectionId,decimal OrignalPrice, bool IsAvailableForInstantOrder , bool IsSpecialOffer, int Stock,string? Location,ProductCondition Condition,IFormFile? ImageFile);
    //public class CreateProductDto
    //{
    //    [Required]
    //    public string Name { get; set; } = string.Empty;
    //    public string Description { get; set; } = string.Empty;
    //    [Range(0.01, double.MaxValue)]
    //    public decimal Price { get; set; }
    //    public int Stock { get; set; }
    //    public string Location { get; set; } = string.Empty;
    //    public int SubCategoryId { get; set; }
    //    public ProductCondition Condition { get; set; } = ProductCondition.NotApplicable;

    //    public IFormFile? ImageFile { get; set; }
    //}
}
