using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Berwah.Models.DTO.Service;
using Berwah.Models.Enum;
using Berwah.Models.Models;
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
            private readonly IImageService _imageService;

        public HomeService(IHomeRepository homeRepository, IImageService imageService)
            {
                _homeRepository = homeRepository;
                _imageService = imageService;
            }

        public async Task<HomeDto> GetHomeDataAsync(string? userId)
        {
            var mainCategories = await _homeRepository.GetMainCategoriesAsync();
            var latestAds = await _homeRepository.GetLatestAdvertisementsAsync(10); 
            var vipBanners = await _homeRepository.GetVipAdvertisementsAsync();     
            var featuredAds = await _homeRepository.GetFeaturedAdvertisementsAsync(); 

            var recentlyViewedProducts = await _homeRepository.GetRecentlyViewedProductsAsync(userId, 10);

            return new HomeDto
            {
                Categories = mainCategories.Select(c => new CategorySummaryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CategoryType = c.CategoryType.ToString()
                }).ToList(),

                Banners = vipBanners.Select(a => new AdvertisementCardDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Price = a.Price,
                    Governorate = a.Governorate,
                    City = a.City,
                    ProductType = a.Condition.ToString(),
                    PromotionPlan = a.PromotionPlan.ToString(),
                    ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url
                }).ToList(),

                Advertisements = featuredAds.Select(a => new AdvertisementCardDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Price = a.Price,
                    Governorate = a.Governorate,
                    City = a.City,
                    ProductType = a.Condition.ToString(),
                    PromotionPlan = a.PromotionPlan.ToString(),
                    ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url
                }).ToList(),

                LatestProducts = latestAds.Select(a => new ProductCardDto
                {
                    Id = a.Id,
                    Name = a.Title,
                    Price = a.Price,
                    ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url,
                    SellerName = a.User?.FirstName
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

        public async Task<WebHomeDto> GetWebHomeDataAsync(string? userId)
        {
            var mainCategories = await _homeRepository.GetMainCategoriesAsync();
            var latestAds = await _homeRepository.GetLatestAdvertisementsAsync(10);
            var vipBanners = await _homeRepository.GetVipAdvertisementsAsync();
            var featuredAds = await _homeRepository.GetFeaturedAdvertisementsAsync();
            var recentlyViewedProducts = await _homeRepository.GetRecentlyViewedProductsAsync(userId, 10);


            var restaurantMerchants = await _homeRepository.GetMerchantsByTypeAsync("Restaurant");
            var groceryMerchants = await _homeRepository.GetMerchantsByTypeAsync("Grocery");

            var serviceSubs = await _homeRepository.GetSubCategoriesByMainCategoryNameAsync("خدمات");

            return new WebHomeDto
            {
                Categories = mainCategories.Select(c => new CategorySummaryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CategoryType = c.CategoryType.ToString()
                }).ToList(),

                Banners = vipBanners.Select(a => new AdvertisementCardDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url
                }).ToList(),

                Advertisements = featuredAds.Select(a => new AdvertisementCardDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Price = a.Price,
                    ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url
                }).ToList(),

                LatestProducts = latestAds.Select(a => new ProductCardDto
                {
                    Id = a.Id,
                    Name = a.Title,
                    Price = a.Price,
                    ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url,
                    SellerName = a.User?.FirstName
                }).ToList(),

                RecentlyViewedProducts = recentlyViewedProducts.Select(p => new ProductCardDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.Image?.Url,
                    SellerName = p.User?.FirstName
                }).ToList(),

                Restaurants = restaurantMerchants.Select(m => new MerchantCardDto
                {
                    Id = m.Id,
                    BusinessName = m.BusinessName ?? $"{m.FirstName} {m.LastName}",
                    BusinessDescription = m.BusinessDescription ?? "",
                    BusinessType = m.BusinessType,
                    MerchantImageUrl = m.Images?.FirstOrDefault()?.Url ?? "",
                    IsOpen = m.AcceptOrdersAutomatically
                }).ToList(),

                Grocerys = groceryMerchants.Select(m => new MerchantCardDto
                {
                    Id = m.Id,
                    BusinessName = m.BusinessName ?? $"{m.FirstName} {m.LastName}",
                    BusinessDescription = m.BusinessDescription ?? "",
                    BusinessType = m.BusinessType,
                    MerchantImageUrl = m.Images?.FirstOrDefault()?.Url ?? "",
                    IsOpen = m.AcceptOrdersAutomatically
                }).ToList(),

                Services = serviceSubs.Select(s => new SubCategoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ImageUrl = ""
                }).ToList()
            };
        }
        public async Task<IEnumerable<MerchantCardDto>> GetHomeMadeFoodAsync()
        {
            var merchants = await _homeRepository.GetHomeMadeFoodMerchantsAsync();

            return merchants.Select(m => new MerchantCardDto
            {
                Id = m.Id,
                BusinessName = m.BusinessName ?? $"{m.FirstName} {m.LastName}",
                BusinessDescription = m.BusinessDescription ?? "",
                BusinessType = m.BusinessType,
                MerchantImageUrl = m.Images?.FirstOrDefault()?.Url ?? "",
                IsOpen = m.AcceptOrdersAutomatically
            }).ToList();
        }

        public async Task<IEnumerable<SubCategoryDto>> GetServicesPageDataAsync()
        {

            var serviceSubs = await _homeRepository.GetSubCategoriesByMainCategoryNameAsync("خدمات");

            return serviceSubs.Select(s => new SubCategoryDto
            {
                Id = s.Id,
                Name = s.Name,
            }).ToList();
        }
        public async Task<bool> CreateServiceRequestAsync(int userId, CreateServiceRequest dto)
        {
            string? attachmentUrl = null;

            if (dto.Attachment != null && dto.Attachment.Length > 0)
            {

                var uploadResult = await _imageService.UploadImageAsync(dto.Attachment);

                if (uploadResult != null)
                {
                    attachmentUrl = uploadResult.Url?.ToString(); 
                }
            } 

                var serviceRequest = new ServiceRequest
            {
                UserId = userId,
                SubCategoryId = dto.SubCategoryId,
                PickupAddress = dto.PickupAddress,
                DeliveryAddress = dto.DeliveryAddress,
                PreferredDate = dto.PreferredDate,
                PreferredTimeSlot = dto.PreferredTimeSlot,
                Description = dto.Description,
                EstimatedPrice = dto.EstimatedPrice,
                AttachmentUrl = attachmentUrl, 
                Status = ServiceRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            return await _homeRepository.AddServiceRequestAsync(serviceRequest);
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetAvailableRequestsForProvidersAsync(int? subCategoryId)
        {
            var requests = await _homeRepository.GetAvailableServiceRequestsAsync(subCategoryId);

            return requests.Select(r => new ServiceRequestDto
            {
                Id = r.Id,
                ClientName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "عميل برواح",
                ClientImage = r.User?.Images?.FirstOrDefault()?.Url ?? "",
                CategoryName = r.SubCategory?.Name ?? "", 
                Description = r.Description,
                Price = r.EstimatedPrice,
                PickupAddress = r.PickupAddress,
                DeliveryAddress = r.DeliveryAddress,
                AttachmentUrl = r.AttachmentUrl,
                TimeAgo = GetTimeAgo(r.CreatedAt)
            }).ToList();
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetAllServiceRequestsAsync()
        {
            var requests = await _homeRepository.GetAllServiceRequestsAsync();

            return requests.Select(r => new ServiceRequestDto
            {
                Id = r.Id,
                ClientName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "عميل برواح",
                ClientImage = r.User?.Images?.FirstOrDefault()?.Url ?? "",
                CategoryName = r.SubCategory?.Name ?? "",
                Description = r.Description,
                Price = r.EstimatedPrice,
                PickupAddress = r.PickupAddress,
                DeliveryAddress = r.DeliveryAddress,
                AttachmentUrl = r.AttachmentUrl,
                TimeAgo = GetTimeAgo(r.CreatedAt)
            }).ToList();
        }
        private string GetTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.UtcNow - createdAt;

            if (timeSpan.TotalSeconds < 60)
            {
                return "منذ لحظات";
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                int minutes = (int)timeSpan.TotalMinutes;
                return minutes == 1 ? "منذ دقيقة" : minutes == 2 ? "منذ دقيقتين" : $"منذ {minutes} دقائق";
            }
            else if (timeSpan.TotalHours < 24)
            {
                int hours = (int)timeSpan.TotalHours;
                return hours == 1 ? "منذ ساعة" : hours == 2 ? "منذ ساعتين" : $"منذ {hours} ساعات";
            }
            else if (timeSpan.TotalDays < 30)
            {
                int days = (int)timeSpan.TotalDays;
                return days == 1 ? "منذ يوم" : days == 2 ? "منذ يومين" : $"منذ {days} أيام";
            }
            else if (timeSpan.TotalDays < 365)
            {
                int months = (int)(timeSpan.TotalDays / 30);
                return months == 1 ? "منذ شهر" : months == 2 ? "منذ شهرين" : $"منذ {months} شهور";
            }
            else
            {
                int years = (int)(timeSpan.TotalDays / 365);
                return years == 1 ? "منذ سنة" : years == 2 ? "منذ سنتين" : $"منذ {years} سنوات";
            }
        }
    }
}

    