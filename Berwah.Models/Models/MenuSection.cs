using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class MenuSection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MerchantId { get; set; }
        public User Merchant { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
