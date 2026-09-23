using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class AdSummaryDto
    {
        public int AdId { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }
        public DateTime PostedDate { get; set; }
    }
}
