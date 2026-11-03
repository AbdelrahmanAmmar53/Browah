using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class MerchantRegister: Registration
    {
        [Required(ErrorMessage = "اسم المقر/المركز مطلوب")]
        public string BusinessName { get; set; } 

        [Required(ErrorMessage = "نوع النشاط مطلوب")]
        public string BusinessType { get; set; }
        [Required(ErrorMessage = "وصف النشاط مطلوب")]
        public string TypeOfService { get; set; }

        public string? BusinessDescription { get; set; }

        public string? DeliveryTimeSpan { get; set; }
        public decimal? MinimumOrder { get; set; }

        //[Required(ErrorMessage = "المحا]
        //public string AddressDetails { get; set; } 

        public string? WorkHours { get; set; }
        public bool AcceptOrdersAutomatically { get; set; }

        /// <summary>Where the shop is, picked by the app (optional). Used as the courier's pickup point.</summary>
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        /// <summary>Comma-separated sub-category ids (e.g. "1009,1010"): the cuisines / sections the store belongs to.</summary>
        public string? SubCategoryIds { get; set; }

        /// <summary>Store picture picked at sign-up (uploaded before the OTP step; only the URL is kept).</summary>
        public IFormFile? ProfileImage { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
