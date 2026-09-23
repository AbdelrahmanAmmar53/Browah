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
    public class HomeService: IHomeService
    {
        private readonly IHomeRepository _homeRepository;
        public HomeService(IHomeRepository homeRepository)
        {
            _homeRepository = homeRepository;
        }
        public async Task<HomeDto> GetHomeDataAsync()
        {
            var mainCategories = await _homeRepository.GetMainCategoriesAsync();
            var latestProducts = await _homeRepository.GetLatestProductsAsync(10);
            var recentlyViewedProducts = await _homeRepository.GetRecentlyViewedProductsAsync(10);
            return new HomeDto
            {
                Categories = mainCategories.Select(c => new CategorySummaryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    //ImageUrl = c.ImageUrl
                }).ToList(),
                LatestProducts = latestProducts.Select(p => new ProductCardDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.Image?.Url
                }).ToList(),
                RecentlyViewedProducts = recentlyViewedProducts.Select(p => new ProductCardDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.Image?.Url
                }).ToList()
            };
        }
    }
}
