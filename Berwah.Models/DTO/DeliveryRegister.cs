using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class DeliveryRegister: Registration
    {
       // public string GovermentOrArea { get; set; } = string.Empty;

        public string? PreviousExperience { get; set; }

        [Required(ErrorMessage = "نوع المركبة مطلوب")]
        public string VehicleType { get; set; } = string.Empty;
        public IFormFile? NationalIdImage { get; set; }
        public IFormFile? DrivingLicenseImage { get; set; }
        public IFormFile? CriminalRecordImage { get; set; }
        public IFormFile? PersonalImage { get; set; }
        public IFormFile? VeicleImage { get; set; }

        public string? PersonalImageUrl { get; set; }
        public string? NationalIdImageUrl { get; set; }
        public string? DrivingLicenseImageUrl { get; set; }
        public string? CriminalRecordImageUrl { get; set; }
        public string? VehicleImageUrl { get; set; }
    }
}
