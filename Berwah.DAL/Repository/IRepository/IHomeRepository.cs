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
        Task<IEnumerable<Advertisment>> GetRecentlyViewedProductsAsync(string userId,int count);
        Task<IEnumerable<User>> GetHomeMadeFoodMerchantsAsync();
        Task<bool> AddServiceRequestAsync(ServiceRequest serviceRequest);
        Task<IEnumerable<ServiceRequest>> GetAvailableServiceRequestsAsync(int? subCategoryId);
        Task<IEnumerable<ServiceRequest>> GetAllServiceRequestsAsync();

        Task<int> GetServiceRequestsCountBySubCategoryIdAsync(int subCategoryId);
        Task<int> GetMerchantsCountByBusinessTypeAsync(string businessTypeName);
        
        /// <summary>
        /// الحصول على معرف التاجر الرئيسي لنوع عمل معين
        /// يُستخدم لإرسال MerchantId مع قائمة الفئات الفرعية للموبايل
        /// </summary>
        Task<int?> GetMerchantIdByBusinessTypeAsync(string businessTypeName);
        
        Task<ServiceOffer> AddAsync(ServiceOffer offer);
        Task<bool> SaveChangesAsync();
    }
}
