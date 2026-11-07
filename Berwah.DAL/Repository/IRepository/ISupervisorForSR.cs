using Berwah.Models.DTO.SupervisorForSR;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Berwah.DAL.Repository.IRepository
{
    public interface ISupervisorForSR
    {
        Task<IEnumerable<ServiceRequest>> GetRequestsByStatusAsync(ServiceRequestStatus status, int page, int pageSize);
        Task<int> GetCountByStatusAsync(ServiceRequestStatus status);
        Task<ServiceRequest?> GetByIdAsync(int id);
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<bool> UpdateAsync(ServiceRequest request);
        Task<ServiceRequest?> GetDetailedRequestAsync(int id);
        Task<IEnumerable<User>> GetAvailableServiceProvidersAsync(string? search, int? subCategoryId);
        Task<bool> ReassignRequestToNewProviderAsync(int requestId, int newProviderId, string reason);
        Task<IEnumerable<ServiceProviderDirectoryDto>> GetServiceProviderDirectoriesAsync(string? search, int? subCategoryId);
        Task<ComplaintStatsDto> GetStatsAsync();
        Task<IEnumerable<Ticket>> GetComplaintsAsync(string status, int page, int pageSize);
        Task<bool> UpdateTicketAsync(Ticket ticket);
        Task<User?> GetProviderProfileByIdAsync(int providerId);
        Task<List<ServiceRequest>> GetProviderCompletedTasksAsync(int providerId, int count);
        Task<int> GetCompletedTaskCountAsync(int providerId);
        Task<double> GetResponseSpeedMinutesAsync(int providerId);
        Task<SupervisorAccountSettingsDto?> GetAccountSettingsAsync(int supervisorId);
    }
}
