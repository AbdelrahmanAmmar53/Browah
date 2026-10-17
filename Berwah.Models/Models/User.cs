using Berwah.Models.Enum;
using Microsoft.AspNetCore.Identity;

namespace Berwah.Models.Models
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string ThirdName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public string Address { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string? PreviousExperience { get; set; }
        public string? VehicleType { get; set; }
        public string? NationalIdImageUrl { get; set; }
        public string? DrivingLicenseImageUrl { get; set; }
        public string? CriminalRecordImageUrl { get; set; }
        public string? VehicleImageUrl { get; set; }
        public string? ProfileImageUrl { get; set; }

        public double Rating { get; set; } = 0;
        public bool HidePhoneNumber { get; set; } = false;
        public bool PushNotificationEnable { get; set; } = true;
        public bool IsFrozen { get; set; } = false;
        public string language { get; set; } = "En";
        public DateTime? LastPasswordChangeDate { get; set; }

        // --- حقول خاصة بالتاجر / المتجر (Merchant / Business) ---
        public string? BusinessName { get; set; }
        public string? BusinessType { get; set; }
        public string? BusinessDescription { get; set; }
        public string? DeliveryTimeSpan { get; set; }
        public decimal? MinimumOrder { get; set; }
        public string? WorkHours { get; set; }
        public bool AcceptOrdersAutomatically { get; set; }


        public UserType UserType { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<UserRole> UserRole { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<VisaCard> VisaCards { get; set; }
    }
}