using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorForAd
{
    public class SupervisorProfileResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Region { get; set; }
        public string Status { get; set; }
        public bool IsAvailable { get; set; }

        public int ExamindPagesCount { get; set; }
        public double VerificationPercentage { get; set; }
        public double ResponseSpeedMinutes { get; set; }
        public double Rating { get; set; }

        public decimal WalledBalance { get; set; }

        public List<SupervisorDocumentDto> Documents { get; set; }
    }
}
