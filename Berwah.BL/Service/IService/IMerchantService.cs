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
        Task<bool> UpdateOrderStatusAsync(int orderId, int merchantId, UpdateMerchantOrderStatusDto model);
        Task<bool> ToggleStoreStatusAsync(int merchantId, bool isOpen);
        Task<RejectOrderDetailsDto?> GetRejectOrderDetailsAsync(int orderId, int merchantId);
        Task<bool> ConfirmOrderRejectionAsync(int orderId, int merchantId, ConfirmRejectOrderDto model);
        Task<MerchantProfileResponseDto?> GetMerchantProfileAsync(int merchantId);
        Task<bool> UpdateWorkHoursAsync(int merchantId, string workHours);
        Task<IEnumerable<MerchantCardDto>> GetMerchantsByBusinessTypeAsync(string businessType);
    }
}
