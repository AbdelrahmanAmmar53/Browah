using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.Models
{
    public class AdvertismentImage
    {
        public bool IsMain { get; set; } = true;
        public int AdvertismentId { get; set; }
        public Advertisment? Advertisment { get; set; }

        public int ImageId { get; set; }
        public Image? Image { get; set; }

    }
}
