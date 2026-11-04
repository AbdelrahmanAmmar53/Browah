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
    public class SupervisorForAdRepository: ISupervisorForAdRepository
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
                .Where(a => a.Status == status)
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
    }
}
