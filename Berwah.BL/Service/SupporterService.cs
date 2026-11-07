using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class SupporterService: ISupporterService
    {
        private readonly ISupporterRepository _supportRepo;

        public SupporterService(ISupporterRepository supportRepo)
        {
            _supportRepo = supportRepo;
        }

        public async Task<TicketSummaryDyo> CreateTicketAsync(int? userId, CreateTicket dto)
        {
            var ticket = new Ticket
            {
                UserId = userId,
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Title = dto.Title,
                Description = dto.Description,
               // TicketNumber = $"BM-{Random.Shared.Next(10000, 99999)}",
                CreatedAt = DateTime.UtcNow,
                Status = TicketStatus.InProgress
            };

            await _supportRepo.CreateTicketAsync(ticket);

            return new TicketSummaryDyo
            {
                //TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                Status = ticket.Status.ToString(),
                CreatedAt = ticket.CreatedAt
            };
        }

        public async Task<IEnumerable<TicketSummaryDyo>> GetMyTicketsAsync(int? userId)
        {
            var tickets = await _supportRepo.GetUserTicketsAsync(userId);
            return tickets.Select(t => new TicketSummaryDyo
            {
                //Id = t.Id,
                //TicketNumber = t.TicketNumber,
                Title = t.Title,
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt
            });
        }

        public async Task<IEnumerable<Models.DTO.Faq>> GetFaqsAsync()
        {
            var faqs = await _supportRepo.GetFaqsAsync();
            return faqs.Select(f => new Models.DTO.Faq
            {
                //Id = f.Id,
                Question = f.Question,
                Answer = f.Answer
            });
        }
        public async Task<Models.DTO.Faq> CreateFaqAsync(CreateFaqDto faq)
        {
            var newFaq = new Berwah.Models.Models.Faq
            {
                Question = faq.Question,
                Answer = faq.Answer
            };
            await _supportRepo.CreateFaqAsync(newFaq);
            return new Models.DTO.Faq
            {
                //Id = newFaq.Id,
                Question = newFaq.Question,
                Answer = newFaq.Answer
            };
        }
    }
}
