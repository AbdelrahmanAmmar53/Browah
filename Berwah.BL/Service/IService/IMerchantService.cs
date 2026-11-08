using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface IMerchantService
    {
        Task<MerchantDashboardResponseDto> GetDashboardDataAsync(int merchantId, string? filter);
        /// <summary>The store's finished orders (handed to the captain, delivered or cancelled), newest first.</summary>
        Task<object> GetOrderHistoryAsync(int merchantId, int page, int pageSize);
        Task<bool> UpdateOrderStatusAsync(int orderId, int merchantId, UpdateMerchantOrderStatusDto model);
        Task<bool> ToggleStoreStatusAsync(int merchantId, bool isOpen);
        Task<RejectOrderDetailsDto?> GetRejectOrderDetailsAsync(int orderId, int merchantId);
        Task<bool> ConfirmOrderRejectionAsync(int orderId, int merchantId, ConfirmRejectOrderDto model);
        Task<MerchantProfileResponseDto?> GetMerchantProfileAsync(int merchantId);
        Task<bool> UpdateWorkHoursAsync(int merchantId, string workHours);
        Task<bool> UpdateLocationAsync(int merchantId, double latitude, double longitude);
        Task<IEnumerable<MerchantCardDto>> GetMerchantsByBusinessTypeAsync(string businessType);
        /// <summary>The store page a customer sees; null when the merchant doesn't exist.</summary>
        Task<StoreMenuDto?> GetStoreMenuAsync(int merchantId);
        /// <summary>The merchant's own items in one of his sections; empty when it isn't his.</summary>
        Task<IEnumerable<ProductCardDto>> GetSectionItemsAsync(int merchantId, int sectionId);
    }
}
