using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface IHomeRepository
    {
        Task<IEnumerable<Category>> GetMainCategoriesAsync();
        Task<IEnumerable<SubCategory>> GetSubCategoriesByMainCategoryNameAsync(string categoryName);
        Task<IEnumerable<Advertisment>> GetLatestAdvertisementsAsync(int count);
        Task<IEnumerable<Advertisment>> GetVipAdvertisementsAsync();
        Task<IEnumerable<Advertisment>> GetFeaturedAdvertisementsAsync();
        Task<IEnumerable<User>> GetMerchantsByTypeAsync(string businessType);
        Task<IEnumerable<Product>> GetRecentlyViewedProductsAsync(string userId,int count);
        Task<IEnumerable<User>> GetHomeMadeFoodMerchantsAsync();
        Task<bool> AddServiceRequestAsync(ServiceRequest serviceRequest);
        Task<IEnumerable<ServiceRequest>> GetAvailableServiceRequestsAsync(int? subCategoryId);
        Task<IEnumerable<ServiceRequest>> GetAllServiceRequestsAsync();
        //Task<IEnumerable<Advertisment>> GetTopAdvertisementsAsync(int count);
    }
}
