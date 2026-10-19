using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.DAL.Repository;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class ProductService: IProductService
    {

        private readonly IProductRepository _repo;
        private readonly ILogger<ProductService> _logger;
        private readonly IImageService _imageService;

        public ProductService(IProductRepository repo, ILogger<ProductService> logger, IImageService imageService)
        {
            _repo = repo;
            _logger = logger;
            _imageService = imageService;
        }

        // 1. جلب كل المنتجات الموجودة في النظام
        public Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken ct = default) =>
            _repo.GetAllProjectedAsync(ct);

        // 2. جلب كل منتجات المستخدم (التاجر) الحالي
        public async Task<IEnumerable<ProductDto>> GetProductsByUserIdAsync(int userId, CancellationToken ct = default)
        {
            var products = await _repo.GetProductsByUserIdAsync(userId);
            return products.Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                "EGP",
                p.ImageUrl ?? string.Empty,
                p.Stock
            ));
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken ct = default)
        {
            var product = await _repo.GetProductByIdAsync(id);
            if (product == null)
                return null;

            return new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                "EGP",
                product.ImageUrl ?? string.Empty,
                product.Stock
            );
        }

        public async Task<ProductDto> CreateProductAsync(int merchantId, CreateProductDto model, CancellationToken ct = default)
        {
            string? imageUrl = null;

            // 1. رفع الصورة على Cloudinary
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadResult = await _imageService.UploadImageAsync(model.ImageFile);
                if (uploadResult != null && uploadResult.SecureUrl != null)
                {
                    imageUrl = uploadResult.SecureUrl.AbsoluteUri;
                }
            }

            // 2. تجهيز كائن الـ Product
            var product = new Product
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? string.Empty : model.Description.Trim(),
                Price = model.Price,
                ImageUrl = imageUrl,
                Location = model.Location,
                Stock = model.Stock,
                Condition = model.Condition,
                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,
                UserId = merchantId,
                CreatedAt = DateTime.UtcNow
            };

            // 3. الحفظ في الداتا بيز عبر الـ Repository (بحيث يرجع الـ entity مع الـ Id الجديد)
            var savedProduct = await _repo.AddAsync(product, ct);

            // 4. إرجاع الـ DTO ليحتوي على الـ Id الذي يطلبه الـ Controller
            return new ProductDto(
                savedProduct.Id,
                savedProduct.Name,
                savedProduct.Description,
                savedProduct.Price,
                "EGP", 
                savedProduct.ImageUrl,
                savedProduct.Stock
            );
        }

        public async Task<bool> UpdateProductAsync(int id, int userId, UpdateProductDto dto, CancellationToken ct = default)
        {
            // 1. جلب المنتج التأكد أنه يخص نفس التاجر
            var existingProduct = await _repo.GetProductByIdAsync(id);
            if (existingProduct == null || existingProduct.UserId != userId)
                return false;

            // 2. رفع صورة جديدة إذا وجدت
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var uploadResult = await _imageService.UploadImageAsync(dto.ImageFile);
                if (uploadResult != null && uploadResult.SecureUrl != null)
                {
                    existingProduct.ImageUrl = uploadResult.SecureUrl.AbsoluteUri;
                }
            }

            // 3. تحديث البيانات
            existingProduct.Name = dto.Name;
            existingProduct.Price = dto.Price;
            existingProduct.Description = string.IsNullOrWhiteSpace(dto.Description) ? string.Empty : dto.Description.Trim();
            existingProduct.Location = dto.Location;
            existingProduct.Stock = dto.Stock;
            existingProduct.CategoryId = dto.CategoryId;
            existingProduct.SubCategoryId = dto.SubCategoryId;
           // existingProduct.Condition = dto.Condition;

            return await _repo.UpdateAsync(existingProduct, ct);
        }

        public async Task<bool> DeleteProductAsync(int id, int userId, CancellationToken ct = default)
        {
            var existingProduct = await _repo.GetProductByIdAsync(id);
            if (existingProduct == null || existingProduct.UserId != userId)
                return false;

            return await _repo.DeleteAsync(existingProduct, ct);
        }

        public Task<IEnumerable<ProductDto>> SearchProductsAsync(SearchProductDto query, CancellationToken ct = default) =>
            _repo.SearchAsync(query, ct);


        // New: return products for a subcategory (menu items)
        public async Task<IEnumerable<ProductCardDto>> GetProductsBySubCategoryAsync(int subCategoryId)
        {
            var products = await _repo.GetProductsBySubCategoryIdAsync(subCategoryId);
            return products.Select(MapToCardDto);
        }

        // New: return menu sections for a main category
        public async Task<IEnumerable<MenuSectionDto>> GetMenuSectionsByMainCategoryIdAsync(int mainCategoryId)
        {
            return await _repo.GetMenuSectionsByMainCategoryIdAsync(mainCategoryId);
        }


        private ProductCardDto MapToCardDto(Product p)
        {
            return new ProductCardDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountedPrice = p.Discount,
                Currency = "EGP",
                Location = p.Location ?? "",
                ImageUrl = p.Image?.Url ?? "",

                // 🟢 ربط بيانات القسم الفرعي والرئيسي
                CategoryId = p.CategoryId,
                SubCategoryId = p.SubCategoryId,
               // SubCategoryName = p.Category?.SubCategories.Name ?? "",
                MainCategoryName = p.Category?.Name ?? "",

                // 🟢 ربط الحالة
                Condition = p.Condition.ToString(),

                // 🟢 ربط بيانات البائع والمستخدم بشكل صحيح
                UserId = p.UserId,
                SellerName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}".Trim() : "",
                IsSellerVerified = p.User?.IsVerified ?? false, // أو أي خاصية توثيق لديك

                Stock = p.Stock > 0,
                CreatedAt = p.CreatedAt
            };
        }
    }
}
