using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        public ProductController(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }
        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(new { status = "success", products });
        }

        [HttpGet("GetProduct/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { status = "fail", message = "Product not found" });

            return Ok(new { status = "success", product });
        }

        [Authorize(Roles = "Admin, User, Merchant")]
        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _productService.CreateProductAsync(userId, dto);
            return Ok(new { status = "success", message = "Product created successfully" });
        }

        [Authorize(Roles = "Admin, User, Merchant")]
        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductDto dto)
        {
            await _productService.UpdateProductAsync(id, dto);
            return Ok(new { status = "success", message = "Product updated successfully" });
        }

        [Authorize(Roles = "Admin, User, Merchant")]
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productService.DeleteProductAsync(id);
            return Ok(new { status = "success", message = "Product deleted successfully" });
        }
    }
}