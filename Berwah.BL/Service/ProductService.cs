using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            // 1. جلب إعدادات العمولة الحالية المحددة من الأدمن
            var settings = await _context.PlatFormSettings.FirstOrDefaultAsync()
                           ?? new PlatFormSetting { CommissionValue = 2.5m, IsPercentage = false };

            decimal commission = 0;

            // 2. حساب قيمة العمولة
            if (settings.IsPercentage)
            {
                commission = dto.Price * (settings.CommissionValue / 100m);
            }
            else
            {
                commission = settings.CommissionValue; 
            }


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

        private ProductCardDto MapToCardDto(Product p)
        {
            return new ProductCardDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountedPrice = p.Discount,
                IsInStock = p.Stock > 0,
                CategoryName = p.Category?.Name ?? "",
                ImageUrl = p.Image?.Url ?? "",
                Location = p.Location,
               // CreatedAt = p.CreatedAt
            };
        }
    }
}
