using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
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
    }
}
