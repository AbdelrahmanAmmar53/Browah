using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.DAL.Repository;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
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
        private readonly IProductRepository _productRepo;
        private readonly ApplicationDbContext _context; 

        public ProductService(IProductRepository productRepo, ApplicationDbContext context)
        {
            _productRepo = productRepo;
            _context = context;
        }

        public async Task<IEnumerable<ProductCardDto>> GetAllProductsAsync()
        {
            var products = await _productRepo.GetAllProductsAsync();
            return products.Select(MapToCardDto);
        }

        public async Task<ProductCardDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepo.GetProductByIdAsync(id);
            if (product == null) return null;

            // تسجيل مشاهدة المنتج عند فتحه تلقائياً
            await _productRepo.TrackViewAsync(id);
            await _productRepo.SaveChangesAsync();

            return MapToCardDto(product);
        }

        public async Task CreateProductAsync(int userId, CreateProductDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            // 1. تحديد حالة المنتج تلقائياً
            ProductCondition finalCondition;

            if (user.UserType == UserType.Merchant)
            {
                finalCondition = ProductCondition.NotApplicable;
            }
            else
            {
                finalCondition = dto.Condition;
            }

            // 2. حساب العمولة
            var settings = await _context.PlatFormSettings.FirstOrDefaultAsync()
                           ?? new PlatFormSetting { CommissionValue = 2.5m, IsPercentage = false };

            decimal commission = settings.IsPercentage
                ? dto.Price * (settings.CommissionValue / 100m)
                : settings.CommissionValue;

            // 3. إنشاء المنتج
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                OriginalPrice = dto.Price,
                CommissionAmount = commission,
                Price = dto.Price + commission,
                Stock = dto.Stock,
                Location = dto.Location,
                SubCategoryId = dto.SubCategoryId,
                Condition = finalCondition,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepo.CreateProductAsync(product);
            await _productRepo.SaveChangesAsync();
        }


        public async Task UpdateProductAsync(int id, CreateProductDto dto)
        {
            var product = await _productRepo.GetProductByIdAsync(id);
            if (product == null) throw new KeyNotFoundException("Product not found");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.Location = dto.Location;

             _productRepo.UpdateProductAsync(product);
            await _productRepo.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepo.GetProductByIdAsync(id);
            if (product == null) throw new KeyNotFoundException("Product not found");

            _productRepo.DeleteProductAsync(product);
            await _productRepo.SaveChangesAsync();
        }

        public async Task<Berwah.Models.DTO.PagedResult<ProductCardDto>> SearchProductsAsync(SearchProductDto searchDto)
        {
            var (products, totalCount) = await _productRepo.SearchProductsAsync(searchDto);

            var productDtos = products.Select(p => MapToCardDto(p)).ToList();

            return new Berwah.Models.DTO.PagedResult<ProductCardDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize)
            };
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
                SubCategoryId = p.SubCategoryId,
                SubCategoryName = p.Category?.Name ?? "",
                MainCategoryName = p.Category?.Category?.Name ?? "",

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
