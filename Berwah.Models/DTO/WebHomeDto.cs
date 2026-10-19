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
        public IEnumerable<MenuSectionDto> RestaurantSections { get; set; } = new List<MenuSectionDto>();
        public IEnumerable<MenuSectionDto> GrocerySections { get; set; } = new List<MenuSectionDto>();
        public IEnumerable<MenuSectionDto> ServiceSections { get; set; } = new List<MenuSectionDto>();
    }
}
