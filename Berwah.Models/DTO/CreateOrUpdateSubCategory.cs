using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class CreateOrUpdateSubCategory
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }
    }
}
