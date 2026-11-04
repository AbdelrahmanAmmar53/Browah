using Berwah.Models.DTO;
using Berwah.Models.DTO.SupervisorForAd;
using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface ISupervisorForAdService
    {
        Task<IEnumerable<AdDetailsResponseDto>> GetAdsByStatusAsync(AdStatus status, int page, int pageSize);
        Task<SupervisorAdStatusDto> GetAdsStatsAsync();
        Task<bool> UpdateAdStatusAsync(int adId, AdStatus status);
        Task<AdDetailsResponseDto?> GetAdDetailsAsync(int id);
        Task<bool> UpdateAdDetailsAsync(int id, UpdateDetailAdDto dto);
        Task<bool> RejectAdWithReasonAsync(int id, RejectAdDto dto);
        Task<bool> HideAdAsync(int id, HideAdDto dto);
        Task<AdReportResponse> GetReportsAsync(string period);
        Task<bool> ExecuteRegulatoryActionAsync(int adId, RegulatoryActionDto dto);




    }
}
