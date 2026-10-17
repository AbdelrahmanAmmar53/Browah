using Berwah.Models.DTO;
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
        Task<IEnumerable<ProductCardDto>> GetAllProductsAsync();
        Task<ProductCardDto?> GetProductByIdAsync(int id);
        Task CreateProductAsync(int userId, CreateProductDto dto);
        Task UpdateProductAsync(int id, CreateProductDto dto);
        Task DeleteProductAsync(int id);
        Task<Berwah.Models.DTO.PagedResult<ProductCardDto>> SearchProductsAsync(SearchProductDto searchDto);
    }
}

