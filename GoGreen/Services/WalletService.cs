using Berwah.DAL.App;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Berwah.Services
{
    /// <summary>
    /// Money movements on the users' wallets, in one place. A wallet is the sum of its lines;
    /// a debit is only written when the balance covers it.
    /// </summary>
    public class WalletService
    {
        public const string KindPayment = "Payment";
        public const string KindRefund = "Refund";
        public const string KindServiceEarning = "ServiceEarning";

        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;

        public WalletService(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<decimal> BalanceAsync(int userId) =>
            await _db.WalletEntries.Where(e => e.UserId == userId).SumAsync(e => (decimal?)e.Amount) ?? 0m;

        /// <summary>Takes <paramref name="amount"/> from the wallet; false (and nothing written) when the balance is short.</summary>
        public async Task<bool> TryChargeAsync(int userId, decimal amount, string description, int? orderId = null, int? serviceRequestId = null)
        {
            amount = Math.Round(amount, 2);
            if (amount <= 0) return true;

            // Serializable: two charges at once cannot spend the same money.
            await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            if (await BalanceAsync(userId) < amount) return false;

            _db.WalletEntries.Add(new WalletEntry
            {
                UserId = userId,
                Amount = -amount,
                Kind = KindPayment,
                Description = description,
                OrderId = orderId,
                ServiceRequestId = serviceRequestId
            });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }

        public async Task CreditAsync(int userId, decimal amount, string kind, string description, int? orderId = null, int? serviceRequestId = null)
        {
            amount = Math.Round(amount, 2);
            if (amount <= 0) return;
            _db.WalletEntries.Add(new WalletEntry
            {
                UserId = userId,
                Amount = amount,
                Kind = kind,
                Description = description,
                OrderId = orderId,
                ServiceRequestId = serviceRequestId
            });
            await _db.SaveChangesAsync();
        }

        /// <summary>What the customer still has held for a service (charged and not given back).</summary>
        public async Task<decimal> HeldForServiceAsync(int serviceRequestId, int customerId)
        {
            var net = await _db.WalletEntries
                .Where(e => e.ServiceRequestId == serviceRequestId && e.UserId == customerId)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;
            return Math.Max(0m, -net);
        }

        /// <summary>Gives the held money back to the customer (the job went back on the table). Safe to call twice.</summary>
        public async Task ReleaseServiceHoldAsync(int serviceRequestId, int customerId)
        {
            // Once the provider was paid there is nothing to give back.
            if (await _db.WalletEntries.AnyAsync(e => e.ServiceRequestId == serviceRequestId && e.Kind == KindServiceEarning))
                return;
            var held = await HeldForServiceAsync(serviceRequestId, customerId);
            if (held > 0)
                await CreditAsync(customerId, held, KindRefund, $"استرداد مبلغ الخدمة #{serviceRequestId}", serviceRequestId: serviceRequestId);
        }

        /// <summary>Pays the provider what the customer held for the finished job, less the commission set in appsettings (0 by default).</summary>
        public async Task PayProviderAsync(int serviceRequestId, int customerId, int providerId)
        {
            if (await _db.WalletEntries.AnyAsync(e => e.ServiceRequestId == serviceRequestId && e.Kind == KindServiceEarning))
                return;
            var held = await HeldForServiceAsync(serviceRequestId, customerId);
            if (held <= 0) return;

            var percent = _config.GetValue<decimal?>("Wallet:ServiceCommissionPercent") ?? 0m;
            percent = Math.Clamp(percent, 0m, 100m);
            var net = Math.Round(held * (100m - percent) / 100m, 2);
            await CreditAsync(providerId, net, KindServiceEarning, $"أرباح الخدمة #{serviceRequestId}", serviceRequestId: serviceRequestId);
        }
    }
}
