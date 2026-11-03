using System;
using System.Collections.Generic;

namespace Berwah.Models.Models
{
    /// <summary>One product in a customer's cart, kept on the server.</summary>
    public class CartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; } = 1;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
