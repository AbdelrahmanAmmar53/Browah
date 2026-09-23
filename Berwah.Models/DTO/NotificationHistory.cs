using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class NotificationHistory
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
