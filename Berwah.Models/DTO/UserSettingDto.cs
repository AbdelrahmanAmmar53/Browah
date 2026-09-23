using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class UserSettingDto
    {
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string Language { get; set; } 
        public bool HidePhoneNumber { get; set; }
        public bool EnableNotifications { get; set; }
    }
}
