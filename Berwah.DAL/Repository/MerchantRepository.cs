using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
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
    public class MerchantRepository : IMerchantRepository
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
                .Include(o => o.Delivery)
                .Where(o => o.OrderItems.Any(oi => oi.Product.UserId == merchantId))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetMerchantOrderWithDetailsAsync(int orderId, int merchantId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.User)
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

        public async Task<List<User>> GetActiveCouriersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Delivery && !u.IsFrozen)
                .ToListAsync();
        }

        public async Task RecordTransactionAsync(int userId, decimal amount, string type, string description)
        {
            await _context.Transactions.AddAsync(new Transaction
            {
                UserId = userId,
                Amount = amount,
                Type = type,
                Description = description,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<int, int>> GetReviewCountsAsync(IEnumerable<int> merchantIds)
        {
            var ids = merchantIds.Distinct().ToList();
            return await _context.StoreReviews
                .AsNoTracking()
                .Where(r => ids.Contains(r.MerchantId))
                .GroupBy(r => r.MerchantId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }

        public async Task RecordWalletRefundAsync(int userId, decimal amount, int orderId, string description)
        {
            if (amount <= 0) return;
            await _context.WalletEntries.AddAsync(new WalletEntry
            {
                UserId = userId,
                Amount = amount,
                Kind = "Refund",
                Description = description,
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetMerchantsByBusinessTypeAsync(string businessType)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Merchant && u.BusinessType != null && u.BusinessType.ToLower() == businessType.ToLower())
                .Include(u => u.Images)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetMerchantsByBusinessTypesAsync(string[] types)
        {
            var wanted = types.Select(t => t.Trim().ToLower()).ToList();
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Merchant
                            && u.BusinessType != null
                            && wanted.Contains(u.BusinessType.Trim().ToLower()))
                .Include(u => u.Images)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetMerchantsExceptBusinessTypesAsync(string[] excludedTypes)
        {
            var excluded = excludedTypes.Select(t => t.Trim().ToLower()).ToList();
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Merchant
                            && (u.BusinessType == null || !excluded.Contains(u.BusinessType.Trim().ToLower())))
                .Include(u => u.Images)
                .ToListAsync();
        }

        public async Task<IEnumerable<MenuSection>> GetMenuSectionsWithActiveProductsAsync(int merchantId)
        {
            var sections = await _context.menuSections
                .AsNoTracking()
                .Where(s => s.MerchantId == merchantId)
                .OrderBy(s => s.Name)
                .ToListAsync();

            foreach (var section in sections)
            {
                section.Products = await _context.Products
                    .AsNoTracking()
                    .Include(p => p.Image)
                    .Where(p => p.MenuSectionId == section.Id && p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            return sections;
        }

        public async Task<IEnumerable<Product>> GetMerchantSectionProductsAsync(int merchantId, int menuSectionId)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Image)
                .Where(p => p.MenuSectionId == menuSectionId && p.MenuSection.MerchantId == merchantId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetMerchantsByCategoryIdAsync(int categoryId)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Merchant)
                .Include(u => u.Images)
                .ToListAsync();
        }

        public async Task<int> GetActiveProductsCountAsync(int merchantId)
        {
            return await _context.Products.CountAsync(p => p.UserId == merchantId && p.IsActive);
        }

        // 🟢 الإضافات الجديدة لتغطية كل دوال الـ Interface الخاصة بأقسام المنيو:

        public async Task<IEnumerable<MenuSection>> GetMerchantMenuSectionsAsync(int merchantId)
        {
            return await _context.menuSections
                .Include(s => s.Products)
                .Where(s => s.MerchantId == merchantId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> AddMenuSectionAsync(MenuSection section)
        {
            await _context.menuSections.AddAsync(section);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Product>> GetProductsByMenuSectionAsync(int menuSectionId)
        {
            return await _context.Products
                .Where(p => p.MenuSectionId == menuSectionId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<MenuSection?> GetMenuSectionByIdAsync(int menuSectionId)
        {
            return await _context.menuSections.FindAsync(menuSectionId);
        }
    }
}