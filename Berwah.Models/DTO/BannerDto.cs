namespace Berwah.Models.DTO
{
    public class BannerDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string? TargetUrl { get; set; }
        public int? TargetProductId { get; set; }
    }
}