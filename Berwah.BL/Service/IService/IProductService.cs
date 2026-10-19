using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface IProductService
    {

        Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken ct = default);
        Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<ProductDto>> GetProductsByUserIdAsync(int userId, CancellationToken ct = default);
        Task<ProductDto> CreateProductAsync(int merchantId, CreateProductDto model, CancellationToken ct = default);
        Task<bool> UpdateProductAsync(int id, int userId, UpdateProductDto dto, CancellationToken ct = default);
        Task<bool> DeleteProductAsync(int id, int userId, CancellationToken ct = default);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(SearchProductDto query, CancellationToken ct = default);
        Task<IEnumerable<ProductCardDto>> GetProductsBySubCategoryAsync(int subCategoryId);
        Task<IEnumerable<MenuSectionDto>> GetMenuSectionsByMainCategoryIdAsync(int mainCategoryId);
   
    }
}

