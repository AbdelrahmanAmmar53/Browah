using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository
{
    public class MerchantRepository: IMerchantRepository
    {
        private readonly ApplicationDbContext _context;
        public MerchantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetMerchantByIdAsync(int merchantId)
        {
            return await _context.Users.FindAsync(merchantId);
        }

        public async Task<List<Order>> GetMerchantOrdersAsync(int merchantId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .Where(o => o.OrderItems.Any(oi => oi.Product.UserId == merchantId))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetMerchantOrderWithDetailsAsync(int orderId, int merchantId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.OrderItems.Any(oi => oi.Product.UserId == merchantId));
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMerchantAsync(User merchant)
        {
            _context.Users.Update(merchant);
            await _context.SaveChangesAsync();
        }

        public async Task RecordMerchantPenaltyTransactionAsync(int merchantId, decimal penaltyAmount, string reason)
        {
            
            var transaction = new Transaction
            {
                UserId = merchantId,
                Amount = -penaltyAmount, 
                Type = "Penalty",        
                Description = reason,    
                CreatedAt = DateTime.UtcNow
            };

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetActiveProductsCountAsync(int merchantId)
        {
            return await _context.Products.CountAsync(p => p.UserId == merchantId && p.IsActive);
        }
    }
}

