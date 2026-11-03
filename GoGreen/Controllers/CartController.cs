using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Berwah.Controllers
{
    /// <summary>
    /// The customer's cart, kept on the server so it follows the account to any phone.
    /// Every call answers with the whole cart: <c>{ status, data: { items, subTotal, totalQuantity } }</c>.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CartController(ApplicationDbContext context) => _context = context;

        private bool IsEnglish =>
            Request.Headers.AcceptLanguage.ToString().StartsWith("en", StringComparison.OrdinalIgnoreCase);

        /// <summary>Why the quantity was refused: sold out, or how many are left.</summary>
        private string StockMessage(int stock) => stock <= 0
            ? (IsEnglish ? "This product is out of stock" : "نفدت كمية هذا المنتج")
            : (IsEnglish ? $"Only {stock} left of this product" : $"المتاح من هذا المنتج {stock} فقط");

        private int? CurrentUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        // GET api/Cart
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (CurrentUserId() is not int userId) return Unauthorized();
            return Ok(new { status = "success", data = await BuildCartAsync(userId) });
        }

        // POST api/Cart/items   { productId, quantity }  — adds, or adds to the quantity already there
        [HttpPost("items")]
        public async Task<IActionResult> Add([FromBody] CartAddDto dto)
        {
            if (CurrentUserId() is not int userId) return Unauthorized();
            if (dto.Quantity < 1) return BadRequest(new { status = "error", message = "الكمية غير صالحة" });

            var error = await AddOrIncreaseAsync(userId, dto.ProductId, dto.Quantity);
            if (error != null) return BadRequest(new { status = "error", message = error });

            await _context.SaveChangesAsync();
            return Ok(new { status = "success", data = await BuildCartAsync(userId) });
        }

        // PUT api/Cart/items/{productId}   { quantity }  — sets the quantity; 0 removes the item
        [HttpPut("items/{productId:int}")]
        public async Task<IActionResult> SetQuantity(int productId, [FromBody] CartSetQuantityDto dto)
        {
            if (CurrentUserId() is not int userId) return Unauthorized();

            var row = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
            if (dto.Quantity <= 0)
            {
                if (row != null) _context.CartItems.Remove(row);
            }
            else
            {
                var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
                if (product == null || !product.IsActive)
                    return BadRequest(new { status = "error", message = "المنتج غير متاح" });
                if (dto.Quantity > product.Stock)
                    return BadRequest(new { status = "error", message = StockMessage(product.Stock) });

                if (row == null)
                    _context.CartItems.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = dto.Quantity });
                else
                    row.Quantity = dto.Quantity;
            }

            await _context.SaveChangesAsync();
            return Ok(new { status = "success", data = await BuildCartAsync(userId) });
        }

        // DELETE api/Cart/items/{productId}
        [HttpDelete("items/{productId:int}")]
        public async Task<IActionResult> Remove(int productId)
        {
            if (CurrentUserId() is not int userId) return Unauthorized();

            var row = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
            if (row != null)
            {
                _context.CartItems.Remove(row);
                await _context.SaveChangesAsync();
            }
            return Ok(new { status = "success", data = await BuildCartAsync(userId) });
        }

        // DELETE api/Cart
        [HttpDelete]
        public async Task<IActionResult> Clear()
        {
            if (CurrentUserId() is not int userId) return Unauthorized();

            var rows = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(rows);
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", data = await BuildCartAsync(userId) });
        }

        // POST api/Cart/merge   { items: [{ productId, quantity }] }
        // Pushes a cart that was filled before sign-in (or before the server had carts):
        // an item missing on the server is added, one already there keeps the larger quantity.
        [HttpPost("merge")]
        public async Task<IActionResult> Merge([FromBody] CartMergeDto dto)
        {
            if (CurrentUserId() is not int userId) return Unauthorized();

            foreach (var item in dto.Items.Where(i => i.Quantity > 0))
            {
                var existing = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == item.ProductId);
                if (existing != null && existing.Quantity >= item.Quantity) continue;

                var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId);
                // A product that is gone or sold out is skipped, not an error.
                if (product == null || !product.IsActive || product.Stock < 1) continue;

                var quantity = Math.Min(item.Quantity, product.Stock);
                if (existing == null)
                    _context.CartItems.Add(new CartItem { UserId = userId, ProductId = item.ProductId, Quantity = quantity });
                else
                    existing.Quantity = quantity;
            }

            await _context.SaveChangesAsync();
            return Ok(new { status = "success", data = await BuildCartAsync(userId) });
        }

        /// <summary>Null when it worked, otherwise the message to show.</summary>
        private async Task<string?> AddOrIncreaseAsync(int userId, int productId, int quantity)
        {
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null || !product.IsActive) return "المنتج غير متاح";

            var row = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
            var wanted = (row?.Quantity ?? 0) + quantity;
            if (wanted > product.Stock) return StockMessage(product.Stock);

            if (row == null)
                _context.CartItems.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = wanted });
            else
                row.Quantity = wanted;
            return null;
        }

        private async Task<CartResponseDto> BuildCartAsync(int userId)
        {
            var rows = await _context.CartItems
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .Include(c => c.Product).ThenInclude(p => p.User)
                .Include(c => c.Product).ThenInclude(p => p.Image)
                .OrderBy(c => c.AddedAt)
                .ToListAsync();

            var items = rows.Select(c =>
            {
                var p = c.Product;
                var seller = p.User == null
                    ? string.Empty
                    : (!string.IsNullOrWhiteSpace(p.User.BusinessName)
                        ? p.User.BusinessName!
                        : $"{p.User.FirstName} {p.User.LastName}".Trim());
                return new CartItemResponseDto
                {
                    ProductId = p.Id,
                    Title = p.Name,
                    Price = p.Price,
                    ImageUrl = !string.IsNullOrWhiteSpace(p.ImageUrl) ? p.ImageUrl : p.Image?.Url,
                    SellerName = seller,
                    Quantity = c.Quantity,
                    Stock = p.Stock,
                    IsAvailable = p.IsActive && p.Stock >= c.Quantity
                };
            }).ToList();

            return new CartResponseDto
            {
                Items = items,
                SubTotal = items.Where(i => i.IsAvailable).Sum(i => i.Price * i.Quantity),
                TotalQuantity = items.Sum(i => i.Quantity)
            };
        }
    }
}
