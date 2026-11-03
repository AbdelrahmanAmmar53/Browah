using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }

        public string? PickupAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }
        public double? DeliveryLatitude { get; set; }
        public double? DeliveryLongitude { get; set; }
        public DateTime? PreferredDate { get; set; }
        public string? PreferredTimeSlot { get; set; }
        public string Description { get; set; }
        public decimal EstimatedPrice { get; set; }
        public string? AttachmentUrl { get; set; }

        public ServiceRequestStatus Status { get; set; }

        /// <summary>The offer the customer chose; the provider who made it does the job.</summary>
        public int? AcceptedOfferId { get; set; }

        /// <summary>The choice the customer made inside the service (vehicle type, kind of work).</summary>
        public int? ServiceOptionId { get; set; }
        /// <summary>When the provider said the job is done; the customer then confirms.</summary>
        public DateTime? ProviderDoneAt { get; set; }

        /// <summary>When the customer confirmed the job (the request is Completed from then).</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>The customer's rating of the provider for this job, 1 to 5.</summary>
        public int? CustomerRating { get; set; }
        public string? CustomerReview { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
