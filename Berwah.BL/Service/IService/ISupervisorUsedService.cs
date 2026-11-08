using Berwah.Models.DTO.SupervisorUsed;
using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface ISupervisorUsedService
    {
        Task<UsedAdStatusDto> GetUsedAdsStatsAsync();
        Task<IEnumerable<object>> GetUsedAdsByStatusAsync(AdStatus? status, int page, int pageSize);
        Task<object?> GetUsedAdDetailsAsync(int id);
        Task<bool> ApproveAndPublishAdAsync(int id, UsedAdActionDto dto);
        Task<bool> RequestEditFromPublisherAsync(int id, string? reason);
        Task<bool> SuspendAdAsync(int id, string? reason);
        Task<bool> RejectAdWithPenaltyAsync(int id, UsedAdActionDto dto);
    }
}
