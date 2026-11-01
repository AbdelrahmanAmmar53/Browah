using Berwah.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface ISupporterService
    {
        Task<TicketSummaryDyo> CreateTicketAsync(int? userId, CreateTicket dto);
        Task<IEnumerable<TicketSummaryDyo>> GetMyTicketsAsync(int? userId);
        Task<IEnumerable<Berwah.Models.DTO.Faq>> GetFaqsAsync();
        Task<Berwah.Models.DTO.Faq> CreateFaqAsync(CreateFaqDto faq);
    }
}
