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
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _context;
        public HomeRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Category>> GetMainCategoriesAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Advertisment>> GetLatestAdvertisementsAsync(int count)
        {
            return await _context.Advertisements
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Include(a => a.User)
                .Include(a => a.Category)
                .AsNoTracking()
                .Where(a => a.Status == AdStatus.Approved)

                .OrderBy(a => a.PromotionPlan == PromotionPlanType.VIP ? 1 :
                              (a.PromotionPlan.ToString().ToLower() == "feature" || a.PromotionPlan.ToString().ToLower() == "featured") ? 2 : 3)

                .ThenByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Advertisment>> GetVipAdvertisementsAsync()
        {
            return await _context.Advertisements
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .AsNoTracking()
                .Where(a => a.PromotionPlan.ToString() == "VIP")
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

      
        public async Task<IEnumerable<Advertisment>> GetFeaturedAdvertisementsAsync()
        {
            return await _context.Advertisements
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Include(a => a.User)
                .AsNoTracking()
        
                .Where(a => a.PromotionPlan == PromotionPlanType.Featured || a.PromotionPlan == PromotionPlanType.Free)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

    
        public async Task<IEnumerable<User>> GetMerchantsByTypeAsync(string businessType)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Merchant &&
                            u.BusinessType != null &&
                            u.BusinessType.ToLower().Contains(businessType.ToLower()))
                .Include(u => u.Images)
                .ToListAsync();
        }


        public async Task<IEnumerable<User>> GetHomeMadeFoodMerchantsAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Merchant &&
                            u.BusinessType != null &&
                            (u.BusinessType.ToLower().Contains("homefood") || u.BusinessType.ToLower().Contains("أكل بيتي")))
                .Include(u => u.Images)
                .ToListAsync();
        }
        public async Task<IEnumerable<SubCategory>> GetSubCategoriesByMainCategoryNameAsync(string categoryName)
        {
            return await _context.SubCategories
                .AsNoTracking()
                .Where(sc => sc.Category != null && sc.Category.Name == categoryName)
                .Include(sc => sc.Category)
                .ToListAsync();
        }

        public async Task<bool> AddServiceRequestAsync(ServiceRequest serviceRequest)
        {
            await _context.ServiceRequests.AddAsync(serviceRequest);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<ServiceRequest>> GetAvailableServiceRequestsAsync(int? subCategoryId)
        {
            var query = _context.ServiceRequests
                .AsNoTracking()
                .Where(r => r.Status == ServiceRequestStatus.Pending);

            if (subCategoryId.HasValue)
            {
                query = query.Where(r => r.SubCategoryId == subCategoryId.Value); 
            }

            return await query
                .Include(r => r.User).ThenInclude(u => u.Images)
                .Include(r => r.SubCategory) 
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        public async Task<IEnumerable<ServiceRequest>> GetAllServiceRequestsAsync()
        {
            return await _context.ServiceRequests
                .AsNoTracking()
                .Where(r => r.Status == ServiceRequestStatus.Pending)
                .Include(r => r.User).ThenInclude(u => u.Images)
                .Include(r => r.SubCategory)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetServiceRequestsCountBySubCategoryIdAsync(int subCategoryId)
        {
            return await _context.ServiceRequests
                .CountAsync(sr => sr.SubCategoryId == subCategoryId);
        }

        public async Task<int> GetMerchantsCountByBusinessTypeAsync(string businessTypeName)
        {
            return await _context.Users
                .CountAsync(u => u.UserType == UserType.Merchant &&
                            u.BusinessType != null &&
                            u.BusinessType.ToLower() == businessTypeName.ToLower());
        }

        public async Task<int?> GetMerchantIdByBusinessTypeAsync(string businessTypeName)
        {
            var merchant = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Merchant &&
                            u.BusinessType != null &&
                            u.BusinessType.ToLower() == businessTypeName.ToLower())
                .OrderByDescending(u => u.CreatedAt)
                .FirstOrDefaultAsync();

            return merchant?.Id;
        }

        public async Task<IEnumerable<Advertisment>> GetRecentlyViewedProductsAsync(string? userId, int count)
        {
            return new List<Advertisment>();
        }

        public async Task<ServiceOffer> AddAsync(ServiceOffer offer)
        {
            await _context.Set<ServiceOffer>().AddAsync(offer);
            return offer;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

    }
}