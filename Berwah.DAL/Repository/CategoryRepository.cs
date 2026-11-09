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
    public class CategoryRepository: ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category?> GetByIdWithSubCategoriesAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.SubCategories) 
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Advertisment>> GetAdminAdvertisementsAsync(int page, int pageSize)
        {
            return await _context.Advertisements
                .Include(a => a.AdvertismentImages).ThenInclude(ai => ai.Image)
                .Include(a => a.Specifications)
                .Where(a =>a.User.UserRole.Any(ur => ur.Role.Name == "Admin"))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Advertisment>> GetBySubCategoryIdAsync(int subCategoryId, int page, int pageSize)
        {
            var ads = await _context.Advertisements
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Include(a => a.Category)
                .Where(a => a.Status == AdStatus.Approved && a.SubCategoryId == subCategoryId && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow) && !(a.User.UserName == "browah" || a.User.UserRole.Any(ur => ur.Role.Name == "Admin")))
                .OrderBy(a => (a.PromotionPlan == PromotionPlanType.VIP && a.PromotionEndsAt > DateTime.UtcNow) ? 0 : (a.PromotionPlan == PromotionPlanType.Featured && a.PromotionEndsAt > DateTime.UtcNow) ? 1 : 2)
                .ThenByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            foreach (var ad in ads)
            {
                ad.Specifications = await _context.AdvertisementSpecifications
                    .Where(s => s.AdvertisementId == ad.Id)
                    .AsNoTracking()
                    .ToListAsync();
            }

            return ads;
        }

        public async Task<IEnumerable<Advertisment>> GetByConditionAsync(int categoryId, string? conditionStr, int page, int pageSize)
        {
            var query = _context.Advertisements
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Include(a => a.Category)
                .Where(a => a.Status == AdStatus.Approved && a.CategoryId == categoryId && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow) && !(a.User.UserName == "browah" || a.User.UserRole.Any(ur => ur.Role.Name == "Admin")));

            if (!string.IsNullOrEmpty(conditionStr) && Enum.TryParse<ProductCondition>(conditionStr, true, out var conditionEnum))
            {
                query = query.Where(a => a.Condition == conditionEnum);
            }

            var ads = await query
                .OrderBy(a => (a.PromotionPlan == PromotionPlanType.VIP && a.PromotionEndsAt > DateTime.UtcNow) ? 0 : (a.PromotionPlan == PromotionPlanType.Featured && a.PromotionEndsAt > DateTime.UtcNow) ? 1 : 2)
                .ThenByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            foreach (var ad in ads)
            {
                ad.Specifications = await _context.AdvertisementSpecifications
                    .Where(s => s.AdvertisementId == ad.Id)
                    .AsNoTracking()
                    .ToListAsync();
            }

            return ads;
        }


        public async Task<IEnumerable<Advertisment>> GetByCategoryIdAsync(int categoryId, int page, int pageSize)
        {
            return await _context.Advertisements
                .Include(a => a.AdvertismentImages).ThenInclude(ai => ai.Image)
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
