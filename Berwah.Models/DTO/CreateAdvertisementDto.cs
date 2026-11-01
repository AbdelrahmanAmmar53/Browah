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
    public class CreateAdvertisementDto
    {
        [Required(ErrorMessage = "عنوان الإعلان مطلوب")]
        [StringLength(200, ErrorMessage = "عنوان الإعلان يجب ألا يتجاوز 200 حرف")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "القسم الرئيسي مطلوب")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "القسم الفرعي مطلوب")]
        public int SubCategoryId { get; set; }

        [Required]
        public ProductCondition Condition { get; set; }

        [Required(ErrorMessage = "المحافظة مطلوبة")]
        public string Governorate { get; set; } = string.Empty;

        [Required(ErrorMessage = "المدينة مطلوبة")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون قيمة موجبة")]
        public decimal Price { get; set; }

        public bool IsNegotiable { get; set; } = false;

        [MaxLength(2000, ErrorMessage = "الوصف يجب ألا يتجاوز 2000 حرف")]
        public string? Description { get; set; }

        [Required]
        public PromotionPlanType PromotionPlan { get; set; } = PromotionPlanType.Free;

        [Required(ErrorMessage = "يرجى إرفاق صورة واحدة على الأقل")]
        public List<IFormFile> Images { get; set; } = new();

        // اختيارية: قائمة المواصفات أثناء الإنشاء
        // public List<SpecificationItemDto> Specifications { get; set; } = new();
        public string? Key { get; set; }
        public string? Value { get; set; }
    }
}
