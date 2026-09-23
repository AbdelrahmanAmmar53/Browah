using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class VerifyOTP
    {
        public string TargetEmail { get; set; }
        public string Otp { get; set; }
    }
}
