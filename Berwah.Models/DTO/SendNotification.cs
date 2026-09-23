using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class SendNotification
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
