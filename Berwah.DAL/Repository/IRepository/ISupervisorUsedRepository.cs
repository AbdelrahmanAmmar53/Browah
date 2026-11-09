using Berwah.Models.DTO.SupervisorUsed;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface ISupervisorUsedRepository
    {
        Task<UsedAdStatusDto> GetStatsAsync();
        Task<IEnumerable<Advertisment>> GetUsedAdsByStatusAsync(AdStatus? status, int page, int pageSize);
        Task<Advertisment?> GetAdByIdAsync(int id);
        Task<bool> UpdateAdAsync(Advertisment ad);
        // 🟢 دوال طلبات وصلني والشكاوى (Wosally Orders & Complaints)
        Task<IEnumerable<Order>> GetWosallyOrdersAsync(string? statusFilter, int page, int pageSize);
        Task<Order?> GetWosallyOrderByIdAsync(int id);
        Task<bool> UpdateOrderAsync(Order order);

        Task<IEnumerable<Ticket>> GetComplaintsAsync(string? statusFilter, int page, int pageSize);
        Task<Ticket?> GetComplaintByIdAsync(int id);
        Task<bool> UpdateTicketAsync(Ticket ticket);
        Task<Ticket?> GetDetailedComplaintAsync(int id);
        Task<IEnumerable<Transaction>> GetWalletTransactionsAsync(int supervisorId);
    }
}
