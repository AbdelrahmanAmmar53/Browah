using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.Models.DTO.SupervisorUsed
{
    public class ComplaintDetailsResponseDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // تفاصيل السلعة المرتبطة بالشكوى
        public ItemDetailsDto? Item { get; set; }

        // أطراف النزاع (المشتري والبائع)
        public PartyDto? Buyer { get; set; }
        public PartyDto? Seller { get; set; }

        // نتائج الفحص الميداني
        public List<string> InspectionChecklist { get; set; } = new();
    }
}
