using Berwah.Models.Enum;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface IAdvertisementRepository
    {
        Task<Advertisment> AddAsync(Advertisment advertisement);
        Task<IEnumerable<Advertisment>> GetByUserIdAsync(int userId);
        Task<Advertisment?> GetByIdAsync(int id);
        Task<IEnumerable<Advertisment>> GetAllAsync();
        void Update(Advertisment advertisement);
        Task DeleteAsync(Advertisment advertisement);
        Task<bool> SaveChangesAsync();

        Task<AdvertisementSpecification> AddSpecificationAsync(AdvertisementSpecification specification);
        Task<IEnumerable<AdvertisementSpecification>> GetSpecificationsByAdvertisementIdAsync(int advertisementId);
        Task DeleteSpecificationAsync(int specificationId);

        Task<int> GetAdvertisementCountByUserIdAsync(int userId);
        Task<List<Advertisment>> GetRecentByUserIdAsync(int userId, int count);

        Task<IEnumerable<Advertisment>> GetBrowahMarketFeedAsync();
        Task<IEnumerable<Advertisment>> GetAdminAdvertisementsAsync(int page, int pageSize);
        Task<IEnumerable<Advertisment>> GetBySubCategoryIdsAsync(List<int> subCategoryIds, int page, int pageSize);
        Task<IEnumerable<Advertisment>> GetByCategoryIdAsync(int categoryId, int page, int pageSize);
    }
}
