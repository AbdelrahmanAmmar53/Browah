using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetByIdWithSubCategoriesAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Advertisment>> GetByCategoryIdAsync(int categoryId, int page, int pageSize);
        Task<IEnumerable<Advertisment>> GetByConditionAsync(int categoryId, string? conditionStr, int page, int pageSize);
        Task<IEnumerable<Advertisment>> GetBySubCategoryIdAsync(int subCategoryId, int page, int pageSize);
        Task<IEnumerable<Advertisment>> GetAdminAdvertisementsAsync(int page, int pageSize);
    }
}
