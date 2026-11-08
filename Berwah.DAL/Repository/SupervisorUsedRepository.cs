using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO.SupervisorUsed;
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
    public class SupervisorUsedRepository: ISupervisorUsedRepository
    {

        private readonly ApplicationDbContext _context;

        public SupervisorUsedRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UsedAdStatusDto> GetStatsAsync()
        {
            var today = DateTime.UtcNow.Date;

            int pendingCount = await _context.Advertisements
                .CountAsync(a => a.Condition == ProductCondition.Used && a.Status == AdStatus.Pending);

            int approvedTodayCount = await _context.Advertisements
                .CountAsync(a => a.Condition == ProductCondition.Used && a.Status == AdStatus.Approved && a.CreatedAt >= today);

            int totalCount = await _context.Advertisements
                .CountAsync(a => a.Condition == ProductCondition.Used);

            return new UsedAdStatusDto
            {
                PendingCount = pendingCount,
                ApprovedTodayCount = approvedTodayCount,
                WaitingCount = pendingCount, 
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<Advertisment>> GetUsedAdsByStatusAsync(AdStatus? status, int page, int pageSize)
        {
            var query = _context.Advertisements
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Where(a => a.Condition == ProductCondition.Used); 

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Advertisment?> GetAdByIdAsync(int id)
        {
            return await _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .FirstOrDefaultAsync(a => a.Id == id && a.Condition == ProductCondition.Used);
        }

        public async Task<bool> UpdateAdAsync(Advertisment ad)
        {
            _context.Advertisements.Update(ad);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
