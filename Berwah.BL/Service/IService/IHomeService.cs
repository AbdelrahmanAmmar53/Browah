using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Berwah.Models.DTO.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface IHomeService
    { 
        Task<HomeDto> GetHomeDataAsync(string userId);
        Task<WebHomeDto> GetWebHomeDataAsync(string? userId);
       // Task<IEnumerable<MerchantCardDto>> GetHomeMadeFoodAsync();
        Task<IEnumerable<SubCategoryDto>> GetServicesPageDataAsync();
        Task<IEnumerable<ServiceRequestDto>> GetAvailableRequestsForProvidersAsync(int? subCategoryId);
        Task<bool> CreateServiceRequestAsync(int userId, CreateServiceRequest dto);
        Task<IEnumerable<ServiceRequestDto>> GetAllServiceRequestsAsync();
        Task<ServiceOfferResponseDto> SubmitOfferAsync(CreateServiceOfferDto dto, int providerId);
    }
}
