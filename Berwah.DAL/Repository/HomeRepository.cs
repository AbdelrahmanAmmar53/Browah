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
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _context;
        public HomeRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Category>> GetMainCategoriesAsync()
        {
            return await _context.Categories
             .AsNoTracking()
             .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetLatestProductsAsync(int count)
        {
            return await _context.Products
                .Include(p => p.Image)
                .Include(p => p.Category)
                .Include(p => p.User)
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetRecentlyViewedProductsAsync(string? userId, int count)
        {
            if (string.IsNullOrEmpty(userId)) return new List<Product>();

            var recentProductGroup = await _context.RecentlyViewedProducts
                .AsNoTracking()
                .Where(pv => pv.UserId == userId)
                .GroupBy(pv => pv.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    MaxViewedAt = g.Max(pv => pv.ViewedAt)
                })
                .OrderByDescending(x => x.MaxViewedAt)
                .Take(count)
                .ToListAsync();

            if (!recentProductGroup.Any())
                return new List<Product>();

            var productIds = recentProductGroup.Select(x => x.ProductId).ToList();

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Image)
                .Include(p => p.Category)
                .Include(p => p.User)
                .ToListAsync();

            return productIds
                .Select(id => products.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)!;
        }

        public async Task<IEnumerable<Advertisment>> GetTopAdvertisementsAsync(int count)
        {
            return await _context.Advertisements
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}