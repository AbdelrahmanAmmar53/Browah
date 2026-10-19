using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext db)
        {
            _context = db;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProjectedAsync(CancellationToken ct = default)
        {
            return await _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProductDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    "EGP",
                    p.ImageUrl ?? string.Empty,
                    p.Stock
                ))
                .ToListAsync(ct);
        }


        public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
        {
            await _context.Products.AddAsync(product, ct);
            await _context.SaveChangesAsync(ct);
            return product;
        }

        public async Task<bool> UpdateAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        public async Task<IEnumerable<ProductDto>> SearchAsync(SearchProductDto query, CancellationToken ct = default)
        {
            var q = _context.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.ProductName))
                q = q.Where(p => EF.Functions.Like(p.Name, $"%{query.ProductName}%"));

            return await q.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Currency, p.ImageUrl, p.Stock))
                          .ToListAsync(ct);
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.Image)
                .Include(p => p.Category)
                    .ThenInclude(sc => sc.SubCategories)
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

        public async Task<IEnumerable<MenuSectionDto>> GetMenuSectionsByMainCategoryIdAsync(int mainCategoryId)
        {
            var sections = await _context.SubCategories
                .AsNoTracking()
                .Where(sc => sc.CategoryId == mainCategoryId)
                .Select(sc => new MenuSectionDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    ProductCount = _context.Products.Count(p => p.SubCategoryId == sc.Id && p.IsActive)
                })
                .OrderBy(sc => sc.Name)
                .ToListAsync();

            return sections;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}