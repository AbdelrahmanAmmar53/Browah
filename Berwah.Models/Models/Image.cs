using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string PublicId { get; set; }
        public string? FileType { get; set; }
        public string? FileName { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<AdvertismentImage> AdvertismentImages { get; set; } = new List<AdvertismentImage>();
    }
}
