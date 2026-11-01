using Berwah.Models.DTO.Merchant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class WebHomeDto : HomeDto
    {
        public IEnumerable<SubCategoryWithCountDto> Restaurants { get; set; } = new List<SubCategoryWithCountDto>();
        public IEnumerable<SubCategoryWithCountDto> Grocerys { get; set; } = new List<SubCategoryWithCountDto>();
        public IEnumerable<SubCategoryWithCountDto> Services { get; set; } = new List<SubCategoryWithCountDto>();
        public IEnumerable<SubCategoryWithCountDto> HomeFood { get; set; } = new List<SubCategoryWithCountDto>();
       // public List<WebCategoriesDetailDto> WebCategoriesDetails { get; set; }
    }
}
