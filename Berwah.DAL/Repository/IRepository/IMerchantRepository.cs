using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface IMerchantRepository
    {
        Task<User?> GetMerchantByIdAsync(int merchantId);
        Task<List<Order>> GetMerchantOrdersAsync(int merchantId);
        Task<Order?> GetMerchantOrderWithDetailsAsync(int orderId, int merchantId);
        Task UpdateOrderAsync(Order order);
        Task UpdateMerchantAsync(User merchant);
        Task RecordMerchantPenaltyTransactionAsync(int merchantId, decimal penaltyAmount, string reason);
        Task<int> GetActiveProductsCountAsync(int merchantId);
        Task<IEnumerable<User>> GetMerchantsByBusinessTypeAsync(string businessType);

        // أقسام المنيو والمنتجات
        Task<IEnumerable<MenuSection>> GetMerchantMenuSectionsAsync(int merchantId);
        Task<bool> AddMenuSectionAsync(MenuSection section);
        Task<IEnumerable<Product>> GetProductsByMenuSectionAsync(int menuSectionId);
        Task<MenuSection?> GetMenuSectionByIdAsync(int menuSectionId);
    }
}
