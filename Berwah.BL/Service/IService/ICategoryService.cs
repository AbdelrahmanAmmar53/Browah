using Berwah.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface ICategoryService
    {
        Task<CategoryContentResponseDto?> GetCategoryContentAsync(int categoryId, int page, int pageSize);
        Task<IEnumerable<AdvertisementResponseDto>> GetFilteredAdsAsync(int categoryId, int? subCategoryId, string? condition, int page, int pageSize);
    }
}
