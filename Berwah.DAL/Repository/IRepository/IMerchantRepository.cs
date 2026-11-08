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
        Task<List<User>> GetActiveCouriersAsync();
        Task RecordTransactionAsync(int userId, decimal amount, string type, string description);
        Task<int> GetActiveProductsCountAsync(int merchantId);
        Task<IEnumerable<User>> GetMerchantsByBusinessTypeAsync(string businessType);
        /// <summary>How many customer reviews each of these stores has.</summary>
        Task<Dictionary<int, int>> GetReviewCountsAsync(IEnumerable<int> merchantIds);
        /// <summary>Merchants whose BusinessType is one of <paramref name="types"/> (trimmed, case-insensitive).</summary>
        Task<IEnumerable<User>> GetMerchantsByBusinessTypesAsync(string[] types);
        /// <summary>Every merchant whose activity is NOT one of the excluded ones (stores registered as "other" count as restaurants).</summary>
        Task<IEnumerable<User>> GetMerchantsExceptBusinessTypesAsync(string[] excludedTypes);
        /// <summary>The merchant's sections, each with its active products (for the customer's store page).</summary>
        Task<IEnumerable<MenuSection>> GetMenuSectionsWithActiveProductsAsync(int merchantId);
        /// <summary>The products of one section, only when the section belongs to <paramref name="merchantId"/>.</summary>
        Task<IEnumerable<Product>> GetMerchantSectionProductsAsync(int merchantId, int menuSectionId);

        // أقسام المنيو والمنتجات
        Task<IEnumerable<MenuSection>> GetMerchantMenuSectionsAsync(int merchantId);
        Task<bool> AddMenuSectionAsync(MenuSection section);
        Task<IEnumerable<Product>> GetProductsByMenuSectionAsync(int menuSectionId);
        Task<MenuSection?> GetMenuSectionByIdAsync(int menuSectionId);
    }
}
