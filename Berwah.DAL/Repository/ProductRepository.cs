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
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Image)
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.Image)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        // Implementation for: Task<IEnumerable<Product>> GetProductsByUserIdAsync(int userId);
        public async Task<IEnumerable<Product>> GetProductsByUserIdAsync(int userId)
        {
            return await _context.Products
                .Where(p => p.UserId == userId)
                .Include(p => p.Image)
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Implementation for: Task<IEnumerable<Product>> GetProductsBySubCategoryIdAsync(int subCategoryId);
        public async Task<IEnumerable<Product>> GetProductsBySubCategoryIdAsync(int subCategoryId)
        {
            return await _context.Products
                .Where(p => p.SubCategoryId == subCategoryId)
                .Include(p => p.Image)
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public void UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
        }

        public void DeleteProductAsync(Product product)
        {
            _context.Products.Remove(product);
        }

        public async Task TrackViewAsync(int productId)
        {
            var view = new ProductViewed
            {
                ProductId = productId,
                ViewedAt = DateTime.UtcNow
            };
            await _context.Set<ProductViewed>().AddAsync(view);
        }

        public async Task<IEnumerable<Product>> GetRecentlyViewedAsync(int count)
        {
            return await _context.Set<ProductViewed>()
                .AsNoTracking()
                .OrderByDescending(v => v.ViewedAt)
                .Select(v => v.Product)
                .Distinct()
                .Include(p => p.Image)
                .Include(p => p.Category)
                .Take(count)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}