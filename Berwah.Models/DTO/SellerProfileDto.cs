namespace Berwah.Models.DTO
{
    public class SellerProfileDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        
        // Statistics
        public int TotalAds { get; set; }
        public int ActiveAds { get; set; }
        public DateTime MemberSince { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        
        // Recent Ads
        public List<AdvertisementCardDto> RecentAdvertisements { get; set; } = new();
    }
}