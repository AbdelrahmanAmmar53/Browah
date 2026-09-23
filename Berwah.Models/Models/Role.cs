using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class Role: IdentityRole<int>
    {
        public Role() { }
        public ICollection<UserRole> UserRole { get; set; }
    }
}
