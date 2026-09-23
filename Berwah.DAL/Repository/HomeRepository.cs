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
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetRecentlyViewedProductsAsync(int count)
        {
            return await _context.RecentlyViewedProducts
                .AsNoTracking()
                .OrderByDescending(pv => pv.ViewedAt)
                .Select(pv => pv.Product)
                .Distinct() // يمنع تكرار نفس المنتج إذا شوهد أكثر من مرة
                .Include(p => p.Image)
                .Include(p => p.Category)
                .Take(count)
                .ToListAsync();
        }
    }
}