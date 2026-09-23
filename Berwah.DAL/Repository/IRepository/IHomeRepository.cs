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
        Task<IEnumerable<Product>> GetLatestProductsAsync(int count);
        Task<IEnumerable<Product>> GetRecentlyViewedProductsAsync(int count);
    }
}
