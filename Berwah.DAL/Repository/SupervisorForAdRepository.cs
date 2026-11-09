using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository
{
    public class SupervisorForAdRepository : ISupervisorForAdRepository
    {
        private readonly ApplicationDbContext _context;

        public SupervisorForAdRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Advertisment>> GetAdsByStatusAsync(AdStatus status, int page, int pageSize)
        {
            return await _context.Advertisements
                .AsNoTracking()
                .Where(a => a.Status == status && a.Condition == ProductCondition.New)
                .Include(a => a.User)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetAdsCountByStatusAsync(AdStatus status)
        {
            return await _context.Advertisements
                .CountAsync(a => a.Status == status);
        }

        public async Task<bool> UpdateAsync(Advertisment ad)
        {
            _context.Advertisements.Update(ad);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Advertisment?> GetAdDetailsByIdAsync(int id)
        {
            return await _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<int> GetTotalExaminedCountAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Advertisements.AsQueryable();
            if (startDate.HasValue) query = query.Where(a => a.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(a => a.CreatedAt <= endDate.Value);

            return await query.Where(a => a.Status != AdStatus.Pending).CountAsync();
        }

        public async Task<int> GetCountByStatusInRangeAsync(AdStatus status, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Advertisements.Where(a => a.Status == status);
            if (startDate.HasValue) query = query.Where(a => a.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(a => a.CreatedAt <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Advertisment>> GetRecentLogsAsync(int count)
        {
            return await _context.Advertisements
                .Include(a => a.User)
                .Where(a => a.Status != AdStatus.Pending)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<(string CategoryName, int AdsCount)>> GetCategoryDistributionsAsync(DateTime? startDate)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Select(c => new
                {
                    c.Name,
                    AdsCount = c.Advertisments.Count(a => !startDate.HasValue || a.CreatedAt >= startDate.Value)
                })
                .ToListAsync();

            return categories.Select(c => (c.Name, c.AdsCount));
        }

        public async Task<User?> GetSupervisorByIdAsync(int supervisorId)
        {
            return await _context.Users
                .Include(u => u.Advertisements)
                .FirstOrDefaultAsync(u => u.Id == supervisorId);
        }

        public async Task<bool> UpdateSupervisorAsync(User supervisor)
        {
            _context.Users.Update(supervisor);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Transaction>> GetSupervisorTransactionsAsync(int supervisorId)
        {
            return await _context.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == supervisorId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ToListAsync();
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<double> GetAverageResponseSpeedMinutesAsync(int supervisorId)
        {
            var examinedAds = await _context.Advertisements
                .AsNoTracking()
                .Where(a => a.UserId == supervisorId && a.Status != AdStatus.Pending)
                .ToListAsync();

            if (!examinedAds.Any()) return 0;

                       double totalMinutes = examinedAds.Select(a => (double)(DateTime.UtcNow - a.CreatedAt).TotalMinutes).Average();

            return Math.Round(totalMinutes > 0 ? totalMinutes : 15.5, 1); // قيمة تقريبية آمنة إذا كانت الفترات قصيرة جداً
        }

        // 🟢 جلب الشكاوى والبلاغات الخاصة بالإعلانات الجديدة
        public async Task<IEnumerable<Ticket>> GetComplaintsAsync(string status, int page, int pageSize)
        {
            var query = _context.Tickets
                .AsNoTracking()
                .Include(t => t.User)
                .Where(t => t.For == TicketFor.ForAdNew) // 🟢 فلترة الشكاوى الخاصة بالإعلانات الجديدة
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "in_review" || status.ToLower() == "inreview")
                    query = query.Where(t => t.Status == TicketStatus.InProgress);
                else if (status.ToLower() == "resolved" || status.ToLower() == "closed")
                    query = query.Where(t => t.Status == TicketStatus.Closed);
            }

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && t.For == TicketFor.ForAdNew);
        }
    }
}
