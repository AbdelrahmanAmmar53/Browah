using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class UpdatePreferencesDto
    {
        public bool? HidePhoneNumber { get; set; }
        public bool? PushNotificationsEnabled { get; set; }
        public string Language { get; set; }
    }
}
