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

        public async Task<Advertisment?> GetByIdAsync(int id)
        {
            return await _context.Advertisements
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> UpdateAsync(Advertisment ad)
        {
            _context.Advertisements.Update(ad);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
