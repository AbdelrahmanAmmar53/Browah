using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Discount { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative integer.")]
        public int Stock { get; set; }
        public string Currency { get; set; } = "EGP";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Location { get; set; }
        //public string ImgUrl { get; set; }
        public int? ImageId { get; set; }
        [AllowNull]
        public Image? Image { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int SubCategoryId { get; set; }
        public SubCategory Category { get; set; }
        [JsonIgnore]
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
