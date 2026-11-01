using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class WebCategoriesDetailDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryType { get; set; }
        public List<SubCategoryWithCountDto> SubCategories { get; set; } = new();
    }
}
