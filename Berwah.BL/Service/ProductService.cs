using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.DAL.Repository;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using DocumentFormat.OpenXml.InkML;
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
    public class ProductService : IProductService
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

        public Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken ct = default) =>
            _repo.GetAllProjectedAsync(ct);

        public async Task<IEnumerable<ProductDto>> GetProductsByUserIdAsync(int userId, CancellationToken ct = default)
        {
            var products = await _repo.GetProductsByUserIdAsync(userId);
            return products.Select(p => new ProductDto(
               p.Id,
               p.Name,
               p.Description,
               p.Price,
               p.OriginalPrice,
               p.IsAvailableForInstantOrder,
               p.IsSpecialOffer,
               p.IsActive,
               "EGP",
               p.ImageUrl ?? string.Empty,
               p.Stock
            )).ToList();
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
                product.OriginalPrice,
                product.IsAvailableForInstantOrder,
                product.IsSpecialOffer,
                product.IsActive,
                "EGP",
                product.ImageUrl ?? string.Empty,
                product.Stock
            );
        }

        public async Task<ProductDto> CreateProductAsync(int merchantId, CreateProductDto model, CancellationToken ct = default)
        {
            string? imageUrl = null;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadResult = await _imageService.UploadImageAsync(model.ImageFile);
                if (uploadResult != null && uploadResult.SecureUrl != null)
                {
                    imageUrl = uploadResult.SecureUrl.AbsoluteUri;
                }
            }

            var product = new Product
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? string.Empty : model.Description.Trim(),
                Price = model.Price,
                OriginalPrice = model.OrignalPrice,
                IsAvailableForInstantOrder = model.IsAvailableForInstantOrder,
                IsSpecialOffer = model.IsSpecialOffer,
                ImageUrl = imageUrl,
                Stock = model.Stock,
                MenuSectionId = model.MenuSectionId, 
                UserId = merchantId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var savedProduct = await _repo.AddAsync(product, ct);

            return new ProductDto(
                savedProduct.Id,
                savedProduct.Name,
                savedProduct.Description,
                savedProduct.Price,
                savedProduct.OriginalPrice,
                savedProduct.IsAvailableForInstantOrder,
                savedProduct.IsSpecialOffer,
                savedProduct.IsActive,
                "EGP",
                savedProduct.ImageUrl,
                savedProduct.Stock
            );
        }

        public async Task<bool> UpdateProductAsync(int id, int userId, UpdateProductDto dto, CancellationToken ct = default)
        {
            var existingProduct = await _repo.GetProductByIdAsync(id);
            if (existingProduct == null || existingProduct.UserId != userId)
                return false;

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var uploadResult = await _imageService.UploadImageAsync(dto.ImageFile);
                if (uploadResult != null && uploadResult.SecureUrl != null)
                {
                    existingProduct.ImageUrl = uploadResult.SecureUrl.AbsoluteUri;
                }
            }

            existingProduct.Name = dto.Name;
            existingProduct.Price = dto.Price;
            existingProduct.OriginalPrice = dto.OriginalPrice;
            existingProduct.IsAvailableForInstantOrder = dto.IsAvailableForInstantOrder;
            existingProduct.IsSpecialOffer = dto.IsSpecialOffer;
            existingProduct.Description = string.IsNullOrWhiteSpace(dto.Description) ? string.Empty : dto.Description.Trim();
            existingProduct.Stock = dto.Stock;
            existingProduct.MenuSectionId = dto.MenuSectionId; 

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

 
        public async Task<IEnumerable<ProductCardDto>> GetProductsByMenuSectionAsync(int menuSectionId)
        {
            var products = await _repo.GetProductsByMenuSectionIdAsync(menuSectionId);
            return products.Select(MapToCardDto);
        }

 
        public async Task<IEnumerable<MenuSectionDto>> GetMerchantMenuSectionsAsync(int merchantId)
        {
            return await _repo.GetMerchantMenuSectionsAsync(merchantId);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByMerchantIdAsync(int merchantId)
        {
            var products = await _repo.GetProductsByMerchantIdAsync(merchantId);

            return products.Select(p => new ProductDto(
               p.Id,
               p.Name,
               p.Description,
               p.Price,
               p.OriginalPrice,
               p.IsAvailableForInstantOrder,
               p.IsSpecialOffer,
               p.IsActive,
               "EGP",
               p.ImageUrl ?? string.Empty,
               p.Stock
           )).ToList();
        }

        public async Task<bool> CreateMenuSectionAsync(int merchantId, CreateMenuSectionDto dto)
        {
            try
            {
                var newSection = new MenuSection
                {
                    Name = dto.Title, 
                    MerchantId = merchantId 
                };

                return await _repo.AddMenuSectionAsync(newSection);
            }
            catch (Exception ex)
            {
                throw new Exception($"حدث خطأ أثناء إنشاء القسم: {ex.Message}");
            }
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
                ImageUrl = p.Image?.Url ?? p.ImageUrl ?? "",
                Condition = p.Condition.ToString(),
                UserId = p.UserId,
                SellerName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}".Trim() : "",
                IsSellerVerified = p.User?.IsVerified ?? false,
                Stock = p.Stock > 0,
                CreatedAt = p.CreatedAt
            };
        }
    }
}
