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
        public IEnumerable<MerchantCardDto> Restaurants { get; set; } = new List<MerchantCardDto>();
        public IEnumerable<MerchantCardDto> Grocerys { get; set; } = new List<MerchantCardDto>();
        public IEnumerable<SubCategoryDto> Services { get; set; } = new List<SubCategoryDto>();
    }
}
