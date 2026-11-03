namespace Berwah.Models.Models
{
    /// <summary>A phone that can receive push notifications for a user (one user may have several).</summary>
    public class DeviceToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        /// <summary>android or ios.</summary>
        public string Platform { get; set; } = "android";
        /// <summary>ar or en: the language the push is written in.</summary>
        public string Language { get; set; } = "ar";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
