using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class SearchProductDto
    {
            public string? ProductName { get; set; }
            public int? MainCategoryId { get; set; }
            public int? SubCategoryId { get; set; }
            public string? Location { get; set; }
            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
    }
}
