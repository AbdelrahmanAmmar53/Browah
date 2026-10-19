using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductDto>> GetAllProjectedAsync(CancellationToken ct = default);
        Task<Product> AddAsync(Product product, CancellationToken ct = default);

        Task<bool> UpdateAsync(Product product, CancellationToken ct = default);
    
        Task<bool> DeleteAsync(Product product, CancellationToken ct = default);
     
        Task<IEnumerable<ProductDto>> SearchAsync(SearchProductDto query, CancellationToken ct = default);
        Task<Product?> GetProductByIdAsync(int productId);

        Task<IEnumerable<Product>> GetProductsByUserIdAsync(int userId);
        
        Task<IEnumerable<Product>> GetProductsBySubCategoryIdAsync(int subCategoryId);
        
        Task TrackViewAsync(int productId);
        
        Task<IEnumerable<Product>> GetRecentlyViewedAsync(int count);
        
        Task<IEnumerable<MenuSectionDto>> GetMenuSectionsByMainCategoryIdAsync(int mainCategoryId);
        
        Task SaveChangesAsync();
    }
}
