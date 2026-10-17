using Berwah.Models.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class CreateOrUpdateCategory
    {
        public string Name { get; set; }
        public IFormFile? IconFile { get; set; }
        public CategoryType CategoryType { get; set; }
    }
}
