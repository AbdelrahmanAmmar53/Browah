using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO
{
    public class FileDataDto
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string? FileName { get; set; } = string.Empty;
        public string? ContentType { get; set; } = string.Empty;
    }
}
