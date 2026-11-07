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
        public string? NationalIdBackImageUrl { get; set; }
        public string? DrivingLicenseImageUrl { get; set; }
        public string? CriminalRecordImageUrl { get; set; }
        public string? VehicleImageUrl { get; set; }
        public string? ProfileImageUrl { get; set; }

        public string? CommercialRegister { get; set; }

        public double Rating { get; set; } = 0;

        // --- Delivery captain ---
        public string? VehiclePlateNumber { get; set; }
        public DateTime? DrivingLicenseExpiry { get; set; }
        /// <summary>How many accepted orders this captain stepped back from (for the acceptance rate).</summary>
        public int ReleasedOrdersCount { get; set; }
        /// <summary>The captain switched the radar off (stored as 'unavailable' so existing accounts stay available).</summary>
        public bool IsUnavailable { get; set; }
        /// <summary>Which license-expiry reminder was sent for the current expiry date (0 none, 1 = 30 days, 2 = 7 days, 3 = expired).</summary>
        public int LicenseReminderStage { get; set; }
        public bool HidePhoneNumber { get; set; } = false;
        public bool PushNotificationEnable { get; set; } = true;
        public bool IsFrozen { get; set; } = false;
        /// <summary>A store that paused itself for a while: closed until this moment (UTC).</summary>
        public DateTime? PausedUntil { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IsPaused => PausedUntil.HasValue && PausedUntil.Value > DateTime.UtcNow;
        public string language { get; set; } = "En";
        public DateTime? LastPasswordChangeDate { get; set; }

        // --- حقول خاصة بالتاجر / المتجر (Merchant / Business) ---
        public string? BusinessName { get; set; }
        public string? BusinessType { get; set; }
        public string? TypeOfService { get; set; }
        /// <summary>Comma-separated sub-category ids the store sells under (e.g. "1009,1010" = grills, fish).</summary>
        public string? BusinessSubCategoryIds { get; set; }
        public string? BusinessDescription { get; set; }
        public string? DeliveryTimeSpan { get; set; }
        public decimal? MinimumOrder { get; set; }
        public string? WorkHours { get; set; }
        public bool AcceptOrdersAutomatically { get; set; }

        public string? ServiceName { get; set; }

        public UserType UserType { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<UserRole> UserRole { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<Advertisment> Advertisements { get; set; }
         public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<VisaCard> VisaCards { get; set; }
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        public virtual ICollection<MenuSection> MenuSections { get; set; }
    }
}