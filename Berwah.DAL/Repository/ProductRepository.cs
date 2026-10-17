using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
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
                    .ThenInclude(sc => sc.Category) 
                .Include(p => p.User)             
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.Image)
                .Include(p => p.Category)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.User)             
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

       
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

        public async Task<(IEnumerable<Product> Items, int TotalCount)> SearchProductsAsync(SearchProductDto searchDto)
        {
            var query = _context.Products
                .Include(p => p.Image)
                .Include(p => p.Category)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.User)
                .AsNoTracking();

            // 1. فلترة بالاسم أو الوصف
            if (!string.IsNullOrWhiteSpace(searchDto.ProductName))
            {
                var searchTerm = searchDto.ProductName.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm));
            }

            // 2. فلترة بالقسم الرئيسي
            if (searchDto.MainCategoryId.HasValue)
            {
                query = query.Where(p => p.Category.CategoryId == searchDto.MainCategoryId.Value);
            }

            // 3. فلترة بالقسم الفرعي
            if (searchDto.SubCategoryId.HasValue)
            {
                query = query.Where(p => p.SubCategoryId == searchDto.SubCategoryId.Value);
            }

            // 4. فلترة بالموقع/المدينة
            if (!string.IsNullOrWhiteSpace(searchDto.Location))
            {
                query = query.Where(p => p.Location.Contains(searchDto.Location));
            }

            // 5. فلترة بالحد الأدنى للسعر (MinPrice) 🟢 [مضاف]
            if (searchDto.MinPrice.HasValue && searchDto.MinPrice.Value > 0)
            {
                query = query.Where(p => p.Price >= searchDto.MinPrice.Value);
            }

            // 6. فلترة بالحد الأقصى للسعر (MaxPrice) 🟢 [مضاف]
            if (searchDto.MaxPrice.HasValue && searchDto.MaxPrice.Value > 0)
            {
                query = query.Where(p => p.Price <= searchDto.MaxPrice.Value);
            }

            // حساب العدد الإجمالي للنتائج قبل الـ Pagination
            var totalCount = await query.CountAsync();

            // تطبيق الـ Pagination والترتيب من الأحدث للأقدم
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}