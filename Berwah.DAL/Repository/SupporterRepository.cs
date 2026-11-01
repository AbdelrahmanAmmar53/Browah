using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository
{
    public class SupporterRepository:ISupporterRepository
    {
        private readonly ApplicationDbContext _context;
        public SupporterRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Ticket>> GetUserTicketsAsync(int? userId)
        {
            return await _context.Tickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateTicketAsync(Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Faq>> GetFaqsAsync()
        {
            return await _context.Faqs.ToListAsync();
        }
        public async Task CreateFaqAsync(Faq faq)
        {
            await _context.Faqs.AddAsync(faq);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserWithDetailsForExportAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Advertisements)
                .Include(u => u.ServiceRequests)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}