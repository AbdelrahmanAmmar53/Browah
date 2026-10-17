using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Berwah.Models.Models
{
    public class AdvertisementSpecification
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Advertisement")]
        public int AdvertisementId { get; set; }
        public Advertisment? Advertisement { get; set; }

        [Required]
        [MaxLength(100)]
        public string SpecificationKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string SpecificationValue { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}