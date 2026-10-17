using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class HomeService : IHomeService
    {
        private readonly IHomeRepository _homeRepository;

        public HomeService(IHomeRepository homeRepository)
        {
            _homeRepository = homeRepository;
        }

        public async Task<HomeDto> GetHomeDataAsync(string? userId)
        {
            var mainCategories = await _homeRepository.GetMainCategoriesAsync();
            var latestProducts = await _homeRepository.GetLatestProductsAsync(10);
            var topAdvertisements = await _homeRepository.GetTopAdvertisementsAsync(3);

            // 🟢 تمرير الـ userId الممرر من الـ Controller
            var recentlyViewedProducts = await _homeRepository.GetRecentlyViewedProductsAsync(userId, 10);

            return new HomeDto
            {
                Categories = mainCategories.Select(c => new CategorySummaryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                   // ImageUrl = c.IconUrl,
                    CategoryType = c.CategoryType.ToString()
                }).ToList(),

                Advertisements = topAdvertisements.Select(a => new AdvertisementCardDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Price = a.Price,
                    Governorate = a.Governorate,
                    City = a.City,
                    ProductType= a.Condition.ToString(),
                    PromotionPlan = a.PromotionPlan.ToString(),
                    ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url
                }).ToList(),

                LatestProducts = latestProducts.Select(p => new ProductCardDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.Image?.Url,
                    SellerName = p.User?.FirstName
                }).ToList(),

                RecentlyViewedProducts = recentlyViewedProducts.Select(p => new ProductCardDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.Image?.Url,
                    SellerName = p.User?.FirstName
                }).ToList()
            };
        }
    }
}