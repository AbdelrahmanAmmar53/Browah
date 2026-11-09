using System;

namespace Berwah.Models.Models
{
    /// <summary>
    /// One line of a customer's wallet. The balance is the sum of <see cref="Amount"/>:
    /// positive for money in (top-up, refund), negative for money out (payment).
    /// </summary>
    public class WalletEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        /// <summary>TopUp, Refund or Payment.</summary>
        public string Kind { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? OrderId { get; set; }
        /// <summary>The service request this line holds money for, or pays out for.</summary>
        public int? ServiceRequestId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>A customer's request to charge the wallet by a transfer the admin checks.</summary>
    public class WalletTopUp
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public decimal Amount { get; set; }
        /// <summary>Key of the transfer method (see the Wallet section of appsettings.json).</summary>
        public string Method { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string ProofImageUrl { get; set; } = string.Empty;
        /// <summary>0 pending, 1 approved, 2 rejected.</summary>
        public int Status { get; set; }
        public string? RejectReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedById { get; set; }
    }
}
