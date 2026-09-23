using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class ProductResponse
    {
        public string Status { get; set; }
        public IEnumerable<ProductItemResponse> Products { get; set; }
    }
}
