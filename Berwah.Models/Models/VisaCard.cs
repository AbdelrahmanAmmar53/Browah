using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class VisaCard
    {
        public int Id { get; set; }
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public bool IsDefault { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
