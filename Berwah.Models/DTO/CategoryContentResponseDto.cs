using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class CategoryContentResponseDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty; 


        public List<SubCategoryDto> SubCategories { get; set; } = new();

       
        public List<object> Items { get; set; } = new();
    }
}

