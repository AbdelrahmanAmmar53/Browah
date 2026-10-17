using Berwah.Models.DTO;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface IProductRepository
    {
        Task CreateProductAsync(Product product);
        void UpdateProductAsync(Product product);
        void DeleteProductAsync(Product product);
        Task<Product> GetProductByIdAsync(int productId);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetProductsByUserIdAsync(int userId);
        Task<IEnumerable<Product>> GetProductsBySubCategoryIdAsync(int subCategoryId);
        Task TrackViewAsync(int productId);
        Task<IEnumerable<Product>> GetRecentlyViewedAsync(int count);
        Task<(IEnumerable<Product> Items, int TotalCount)> SearchProductsAsync(SearchProductDto searchDto);
        Task SaveChangesAsync();
    }
}
