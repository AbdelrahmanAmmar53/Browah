using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO.SupervisorUsed;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository
{
    public class SupervisorUsedRepository: ISupervisorUsedRepository
    {

        private readonly ApplicationDbContext _context;

        public SupervisorUsedRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UsedAdStatusDto> GetStatsAsync()
        {
            var today = DateTime.UtcNow.Date;

            int pendingCount = await _context.Advertisements
                .CountAsync(a => a.Condition == ProductCondition.Used && a.Status == AdStatus.Pending);

            int approvedTodayCount = await _context.Advertisements
                .CountAsync(a => a.Condition == ProductCondition.Used && a.Status == AdStatus.Approved && a.CreatedAt >= today);

            int totalCount = await _context.Advertisements
                .CountAsync(a => a.Condition == ProductCondition.Used);

            return new UsedAdStatusDto
            {
                PendingCount = pendingCount,
                ApprovedTodayCount = approvedTodayCount,
                WaitingCount = pendingCount, 
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<Advertisment>> GetUsedAdsByStatusAsync(AdStatus? status, int page, int pageSize)
        {
            var query = _context.Advertisements
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .Where(a => a.Condition == ProductCondition.Used); 

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Advertisment?> GetAdByIdAsync(int id)
        {
            return await _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .FirstOrDefaultAsync(a => a.Id == id && a.Condition == ProductCondition.Used);
        }

        public async Task<bool> UpdateAdAsync(Advertisment ad)
        {
            _context.Advertisements.Update(ad);
            return await _context.SaveChangesAsync() > 0;
        }

        // --- تنفيذ طلبات وصلني ---
        public async Task<IEnumerable<Order>> GetWosallyOrdersAsync(string? statusFilter, int page, int pageSize)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.User) 
                                      
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                // فلترة حسب الحالة إن وجدت
            }

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Order?> GetWosallyOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        // --- تنفيذ الشكاوي والبلاغات ---
        public async Task<IEnumerable<Ticket>> GetComplaintsAsync(string? statusFilter, int page, int pageSize)
        {
            var query = _context.Tickets
                .AsNoTracking()
                .Include(t => t.User)
                .Where(T => T.For == TicketFor.ForUsed)   
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter.ToLower() == "in_review" || statusFilter.ToLower() == "inreview")
                    query = query.Where(t => t.Status == TicketStatus.InProgress);
                else if (statusFilter.ToLower() == "resolved" || statusFilter.ToLower() == "closed")
                    query = query.Where(t => t.Status == TicketStatus.Closed);
            }

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Ticket?> GetComplaintByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && t.For == TicketFor.ForUsed);
        }

        public async Task<bool> UpdateTicketAsync(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Ticket?> GetDetailedComplaintAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.User) // المشتري أو صاحب الشكوى
                .Include(t => t.Advertisement) // لو الشكوى مرتبطة بإعلان
                    .ThenInclude(a => a.AdvertismentImages)
                        .ThenInclude(ai => ai.Image)
                .Include(t => t.Advertisement)
                    .ThenInclude(a => a.User) // البائع
                .FirstOrDefaultAsync(t => t.Id == id && t.For == TicketFor.ForUsed);
        }

        public async Task<IEnumerable<Transaction>> GetWalletTransactionsAsync(int supervisorId)
        {
            return await _context.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == supervisorId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ToListAsync();
        }
    }
}
