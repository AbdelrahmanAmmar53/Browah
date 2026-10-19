using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService) => _productService = productService;

        [HttpGet("GetAllProducts")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts(CancellationToken ct = default)
        {
            var products = await _productService.GetAllProductsAsync(ct);
            return Ok(new { status = "success", data = products });
        }

        [Authorize]
        [HttpGet("my-products")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetMyProducts(CancellationToken ct = default)
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var userId))
                return Unauthorized(new { status = "fail", message = "Invalid user" });

            var products = await _productService.GetProductsByUserIdAsync(userId, ct);
            return Ok(new { status = "success", data = products });
        }

        [HttpGet("{id:int}", Name = nameof(GetProduct))]
        public async Task<ActionResult<ProductDto>> GetProduct(int id, CancellationToken ct = default)
        {
            var product = await _productService.GetProductByIdAsync(id, ct);
            return product is not null
                ? Ok(new { status = "success", data = product })
                : NotFound(new { status = "fail", message = "Product not found" });
        }

        [Authorize]
        [HttpPost("CreateProduct")]
        public async Task<ActionResult> CreateProduct([FromForm] CreateProductDto dto, CancellationToken ct = default)
        {
            if (dto is null) return BadRequest(new { status = "fail", message = "Invalid payload" });

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var userId)) return Unauthorized(new { status = "fail", message = "Invalid user" });

            var created = await _productService.CreateProductAsync(userId, dto, ct);
            // returns created id or ProductDto; using CreatedAtAction is more RESTful
            return CreatedAtRoute(nameof(GetProduct), new { id = created.Id }, new { status = "success", data = created });
        }


        [Authorize]
        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto, CancellationToken ct = default)
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var userId)) return Unauthorized(new { status = "fail", message = "Invalid user" });

            var updated = await _productService.UpdateProductAsync(id, userId, dto, ct);
            if (!updated)
                return NotFound(new { status = "fail", message = "Product not found or not updated" });

            return Ok(new { status = "success", message = "Product updated successfully" });
        }

        [Authorize]
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken ct = default)
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var userId)) return Unauthorized(new { status = "fail", message = "Invalid user" });

            var deleted = await _productService.DeleteProductAsync(id, userId, ct);
            if (!deleted)
                return NotFound(new { status = "fail", message = "Product not found or not deleted" });

            return Ok(new { status = "success", message = "Product deleted successfully" });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SearchProductDto searchDto)
        {
            var result = await _productService.SearchProductsAsync(searchDto);
            return Ok(new
            {
                status = "success",
                data = result
            });
        }
    }
}