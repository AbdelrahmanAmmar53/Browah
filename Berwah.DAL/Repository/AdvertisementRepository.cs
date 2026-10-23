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
    public class AdvertisementRepository : IAdvertisementRepository
    {
        private readonly ApplicationDbContext _context;

        public AdvertisementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Advertisment> AddAsync(Advertisment advertisement)
        {
            await _context.Advertisements.AddAsync(advertisement);
            return advertisement;
        }

        public async Task<Advertisment?> GetByIdAsync(int id)
        {
            var ad = await _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ad != null)
            {
                ad.Specifications = await _context.AdvertisementSpecifications
                    .Where(s => s.AdvertisementId == id)
                    .ToListAsync();
            }

            return ad;
        }

        public async Task<IEnumerable<Advertisment>> GetByUserIdAsync(int userId)
        {

            var ads = await _context.Advertisements
                .Where(a => a.UserId == userId)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            foreach (var ad in ads)
            {
                ad.Specifications = await _context.AdvertisementSpecifications
                    .Where(s => s.AdvertisementId == ad.Id)
                    .ToListAsync();
            }

            return ads;
        }

        public async Task<IEnumerable<Advertisment>> GetAllAsync()
        {
            return await _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Include(a => a.Specifications) 
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public void Update(Advertisment advertisement)
        {
            _context.Advertisements.Update(advertisement);
        }

        public void Delete(Advertisment advertisement)
        {
            _context.Advertisements.Remove(advertisement);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<AdvertisementSpecification> AddSpecificationAsync(AdvertisementSpecification specification)
        {
            await _context.AdvertisementSpecifications.AddAsync(specification);
            return specification;
        }

        public async Task<IEnumerable<AdvertisementSpecification>> GetSpecificationsByAdvertisementIdAsync(int advertisementId)
        {
            return await _context.AdvertisementSpecifications
                .Where(s => s.AdvertisementId == advertisementId)
                .AsNoTracking()
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteSpecificationAsync(int specificationId)
        {
            var spec = await _context.AdvertisementSpecifications.FindAsync(specificationId);
            if (spec != null)
            {
                _context.AdvertisementSpecifications.Remove(spec);
                await SaveChangesAsync();
            }
        }

        public async Task<int> GetAdvertisementCountByUserIdAsync(int userId)
        {
            return await _context.Advertisements
                .Where(a => a.UserId == userId)
                .CountAsync();
        }

        public async Task<List<Advertisment>> GetRecentByUserIdAsync(int userId, int count)
        {
            return await _context.Advertisements
                .Where(a => a.UserId == userId)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}


