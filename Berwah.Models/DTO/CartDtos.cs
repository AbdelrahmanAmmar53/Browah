using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

namespace Berwah.Models.DTO
{
    public class CartItemResponseDto
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        /// <summary>Stock left; the app caps the quantity with it.</summary>
        public int Stock { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CartResponseDto
    {
        public List<CartItemResponseDto> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class CartAddDto
    {
        public int ProductId { get; set; }
        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;
    }

    public class CartSetQuantityDto
    {
        /// <summary>0 or less removes the item.</summary>
        public int Quantity { get; set; }
    }

    /// <summary>The whole local cart, pushed once after sign-in to merge it with the server's.</summary>
    public class CartMergeDto
    {
        public List<CartAddDto> Items { get; set; } = new();
    }
}
