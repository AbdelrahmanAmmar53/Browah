using Berwah.Models.Enum;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public string IconUrl { get; set; }
        public CategoryType CategoryType { get; set; }
       // public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public ICollection<Advertisment> Advertisments { get; set; } = new List<Advertisment>();
    }
}
