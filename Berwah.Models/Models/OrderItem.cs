using System.ComponentModel.DataAnnotations.Schema;

namespace Berwah.Models.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } 
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// This item's store's own progress (Processing / Shipped = ready for the courier).
        /// Null on orders made before stores could share an order: the order's status applies.
        /// </summary>
        public Berwah.Models.Enum.OrderStatus? MerchantStatus { get; set; }

        /// <summary>When the courier collected this item from its store (null = not yet).</summary>
        public DateTime? PickedUpAt { get; set; }
    }
}