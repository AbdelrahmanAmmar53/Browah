using Berwah.Models.Enum;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class User: IdentityUser<int>
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string ThirdName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        //public string Password { get; set; }
        //public DateTime BirthDay { get; set; }
        // public string Gender { get; set; }
        public string Address { get; set; }
        public UserType UserType { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<UserRole> UserRole { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
