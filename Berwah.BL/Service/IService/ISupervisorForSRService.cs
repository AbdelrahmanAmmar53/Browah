using Berwah.Models.DTO.Service;
using Berwah.Models.DTO.SupervisorForSR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service.IService
{
    public interface ISupervisorForSRService
    {
        Task<IEnumerable<ServiceRequestDetailsDto>> GetPendingRequestsAsync(int page, int pageSize);
        Task<SupervisorServiceStatsDto> GetStatsAsync();
        Task<bool> ApproveRequestAsync(int id, ApproveServiceRequestDto dto);
        Task<bool> RejectRequestAsync(int id, RejectServiceRequestDto dto);
        Task<ServiceRequestDetailsDto?> GetRequestDetailsAsync(int id);
        Task<bool> UpdateAndApproveRequestAsync(int id, SupervisorUpdateServiceRequestDto dto);
        Task<IEnumerable<ServiceProviderDirectoryDto>> GetServiceProvidersDirectoryAsync(string? search, int? subCategoryId);
        Task<bool> ReassignRequestAsync(int requestId, ReassignServiceRequestDto dto);
        Task<IEnumerable<ComplaintItemDto>> GetComplaintsAsync(string status, int page, int pageSize);
        Task<bool> EscalateComplaintAsync(int id, ComplaintActionDto dto);
        Task<bool> EnforceCompensationAsync(int id, ComplaintActionDto dto);
        Task<ProviderProfessionalProfileResponseDto?> GetProviderProfessionalProfileAsync(int providerId);
        Task<SupervisorAccountSettingsDto?> GetAccountSettingsAsync(int supervisorId);

    }
}
