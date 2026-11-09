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

        public async Task DeleteAsync(Advertisment advertisement)
        {

            var specs = await _context.AdvertisementSpecifications
                .Where(s => s.AdvertisementId == advertisement.Id)
                .ToListAsync();

            if (specs.Any())
            {
                _context.AdvertisementSpecifications.RemoveRange(specs);
            }

            var adImages = await _context.AdvertismentImages
                .Where(ai => ai.AdvertismentId == advertisement.Id) 
                .ToListAsync();

            if (adImages.Any())
            {
                _context.AdvertismentImages.RemoveRange(adImages);
            }


            _context.Advertisements.Remove(advertisement);

            await _context.SaveChangesAsync();
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

        public async Task<IEnumerable<Advertisment>> GetBrowahMarketFeedAsync()
        {
            return await _context.Advertisements
                .Include(a => a.User)
                    .ThenInclude(u => u.UserRole)          
                        .ThenInclude(ur => ur.Role)       
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Include(a => a.Specifications)
                .Where(a => a.User.UserName == "browah" || a.User.UserRole.Any(ur => ur.Role.Name == "Admin")) 
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // أضف هذه الدوال داخل AdvertisementRepository.cs

        public async Task<IEnumerable<Advertisment>> GetAdminAdvertisementsAsync(int page, int pageSize)
        {
            return await _context.Advertisements
                .Include(a => a.User)
                    .ThenInclude(u => u.UserRole)
                        .ThenInclude(ur => ur.Role)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Include(a => a.Specifications)
                // فلترة إعلانات الأدمن بناءً على منطق سوق برواح لديك
                .Where(a => a.User.UserName == "browah" || a.User.UserRole.Any(ur => ur.Role.Name == "Admin"))
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Advertisment>> GetBySubCategoryIdsAsync(List<int> subCategoryIds, int page, int pageSize)
        {
            return await _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Include(a => a.Specifications)
                .Where(a => a.Status == AdStatus.Approved && subCategoryIds.Contains(a.SubCategoryId) && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow) && !(a.User.UserName == "browah" || a.User.UserRole.Any(ur => ur.Role.Name == "Admin")))
                .OrderBy(a => (a.PromotionPlan == PromotionPlanType.VIP && a.PromotionEndsAt > DateTime.UtcNow) ? 0 : (a.PromotionPlan == PromotionPlanType.Featured && a.PromotionEndsAt > DateTime.UtcNow) ? 1 : 2)
                .ThenByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Advertisment>> GetByCategoryIdAsync(int categoryId, int page, int pageSize)
        {
            return await _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Include(a => a.Specifications)
                .Where(a => a.Status == AdStatus.Approved && a.CategoryId == categoryId && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow) && !(a.User.UserName == "browah" || a.User.UserRole.Any(ur => ur.Role.Name == "Admin")))
                .OrderBy(a => (a.PromotionPlan == PromotionPlanType.VIP && a.PromotionEndsAt > DateTime.UtcNow) ? 0 : (a.PromotionPlan == PromotionPlanType.Featured && a.PromotionEndsAt > DateTime.UtcNow) ? 1 : 2)
                .ThenByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}


