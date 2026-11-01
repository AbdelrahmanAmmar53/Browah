using Berwah.Models.DTO;
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
    }
}
