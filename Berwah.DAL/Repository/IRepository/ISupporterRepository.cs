using Berwah.Models.DTO;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface ISupporterRepository
    {
        Task<IEnumerable<Ticket>> GetUserTicketsAsync(int? userId);
        Task CreateTicketAsync(Ticket ticket);
        Task<IEnumerable<Berwah.Models.Models.Faq>> GetFaqsAsync();

    }
}
