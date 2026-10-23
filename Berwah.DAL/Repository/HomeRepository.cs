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
                .OrderByDescending(a => a.CreatedAt)
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
                .AsNoTracking()
                .Where(a => a.PromotionPlan.ToString() == "Featured")
                .OrderByDescending(a => a.CreatedAt)
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

        public async Task<IEnumerable<User>> GetMerchantsByTypeAsync(string businessType)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Merchant && u.BusinessType != null && u.BusinessType.ToLower() == businessType.ToLower())
                .Include(u => u.Images)
                .ToListAsync();
        }
        public async Task<IEnumerable<User>> GetHomeMadeFoodMerchantsAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Merchant && u.BusinessType != null && u.BusinessType.ToLower() == "homemadefood") 
                .Include(u => u.Images)
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

        public async Task<IEnumerable<Product>> GetRecentlyViewedProductsAsync(string? userId, int count)
        {
            return new List<Product>();
        }
    }
}