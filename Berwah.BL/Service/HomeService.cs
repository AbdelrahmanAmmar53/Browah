using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Berwah.Models.DTO.Service;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
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

        private readonly INotificationService _notifications;
        private readonly ApplicationDbContext _db;

        public HomeService(IHomeRepository homeRepository, IImageService imageService, INotificationService notifications, ApplicationDbContext db)
            {
                _homeRepository = homeRepository;
                _imageService = imageService;
                _notifications = notifications;
                _db = db;
            }
        private async Task SafeNotifyAsync(int userId, string title, string message)
        {
            try { await _notifications.SendNotificationAsync(new SendNotification { UserId = userId, Title = title, Message = message }); }
            catch { /* a failed notification must never fail the action */ }
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
                    PromotionPlan = a.EffectivePlan.ToString(),
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
                    PromotionPlan = a.EffectivePlan.ToString(),
                    ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url
                }).ToList(),

                LatestProducts = latestAds.Select(a => new AdvertisementCardDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Price = a.Price,
                    Governorate = a.Governorate,
                    City = a.City,
                    ProductType = a.Condition.ToString(),
                    PromotionPlan = a.EffectivePlan.ToString(),
                    ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url,
                    SellerName = a.User?.FirstName
                }).ToList(),

                RecentlyViewedProducts = recentlyViewedProducts.Select(p => new AdvertisementCardDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Governorate = p.Governorate,
                    City = p.City,
                    ProductType = p.Condition.ToString(),
                    PromotionPlan = p.EffectivePlan.ToString(),
                    ImageUrl = p.AdvertismentImages?.FirstOrDefault()?.Image?.Url,
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

            var restaurantSubs = new List<SubCategoryWithCountDto>();
            var grocerySubs = new List<SubCategoryWithCountDto>();
            var servicesSubs = new List<SubCategoryWithCountDto>();
            var homeFoodSubs = new List<SubCategoryWithCountDto>();

            
            foreach (var cat in mainCategories)
            {
                if (cat.CategoryType == CategoryType.Restaurant ||
                    cat.CategoryType == CategoryType.Grocery ||
                    cat.CategoryType == CategoryType.HomeFood ||
                    cat.CategoryType == CategoryType.Services ||
                    cat.Name.Contains("اكل بيتي") || cat.Name.Contains("أكل بيتي"))
                {
                    var subs = await _homeRepository.GetSubCategoriesByMainCategoryNameAsync(cat.Name);
                    var subCategoriesWithCounts = new List<SubCategoryWithCountDto>();

                    foreach (var sub in subs)
                    {
                        int availableCount = 0;
                        if (cat.CategoryType == CategoryType.Services)
                        {
                            availableCount = await _homeRepository.GetServiceRequestsCountBySubCategoryIdAsync(sub.Id);
                        }
                        else
                        {
                            availableCount = await _homeRepository.GetMerchantsCountByBusinessTypeAsync(sub.Name);
                        }

                        subCategoriesWithCounts.Add(new SubCategoryWithCountDto
                        {
                            SubCategoryId = sub.Id,
                            SubCategoryName = sub.Name,
                            Count = availableCount
                        });
                    }

                    
                    if (cat.CategoryType == CategoryType.Restaurant || cat.Name.Contains("مطاعم"))
                    {
                        restaurantSubs = subCategoriesWithCounts;
                    }
                    else if (cat.CategoryType == CategoryType.Grocery || cat.Name.Contains("البقالة"))
                    {
                        grocerySubs = subCategoriesWithCounts;
                    }
                    else if (cat.CategoryType == CategoryType.Services || cat.Name.Contains("خدمات"))
                    {
                        servicesSubs = subCategoriesWithCounts;
                    }
                    else if (cat.CategoryType == CategoryType.HomeFood || cat.Name.Contains("اكل بيتي") || cat.Name.Contains("أكل بيتي"))
                    {
                        homeFoodSubs = subCategoriesWithCounts;
                    }
                }
            }

            return new WebHomeDto
            {
                Restaurants = restaurantSubs,
                Grocerys = grocerySubs,
                Services = servicesSubs,
                HomeFood = homeFoodSubs,

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

                // إعلانات الـ Feature
                Advertisements = featuredAds.Select(a => new AdvertisementCardDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Price = a.Price,
                    ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url
                }).ToList(),

                LatestProducts = latestAds.Select(a => new AdvertisementCardDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Price = a.Price,
                    ImageUrl = a.AdvertismentImages?.FirstOrDefault()?.Image?.Url,
                    SellerName = a.User?.FirstName
                }).ToList(),

                RecentlyViewedProducts = recentlyViewedProducts.Select(p => new AdvertisementCardDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Price = p.Price,
                    ImageUrl = p.AdvertismentImages?.FirstOrDefault()?.Image?.Url,
                    SellerName = p.User?.FirstName
                }).ToList()
            };
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
                ServiceOptionId = dto.ServiceOptionId,
                PickupAddress = dto.PickupAddress,
                DeliveryAddress = dto.DeliveryAddress,
                PickupLatitude = dto.PickupLatitude,
                PickupLongitude = dto.PickupLongitude,
                DeliveryLatitude = dto.DeliveryLatitude,
                DeliveryLongitude = dto.DeliveryLongitude,
                PreferredDate = dto.PreferredDate,
                PreferredTimeSlot = dto.PreferredTimeSlot,
                Description = dto.Description,
                EstimatedPrice = dto.EstimatedPrice,
                AttachmentUrl = attachmentUrl, 
                Status = ServiceRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var added = await _homeRepository.AddServiceRequestAsync(serviceRequest);
            if (added)
            {
                // مشرفو الخدمات يعرفوا فورًا إن في طلب بانتظار المراجعة.
                try
                {
                    var supervisorIds = await _db.Users
                        .Where(u => u.UserRole.Any(ur => ur.Role.Name == "SupervisorForServices"))
                        .Select(u => u.Id)
                        .ToListAsync();
                    foreach (var supervisorId in supervisorIds)
                        await _notifications.SendNotificationAsync(new SendNotification
                        {
                            UserId = supervisorId,
                            Title = "طلب خدمة جديد بانتظار المراجعة",
                            Message = $"وصل الطلب #SRV-{serviceRequest.Id} وبانتظار قرارك.",
                            Type = "service_review",
                            RefId = serviceRequest.Id
                        });
                }
                catch { }
            }
            return added;
        }

        /// <summary>Fills each request's option name (vehicle type, kind of work) in one query.</summary>
        private async Task<IEnumerable<ServiceRequestDto>> WithOptionNamesAsync(List<ServiceRequestDto> list)
        {
            var ids = list.Where(r => r.ServiceOptionId != null).Select(r => r.ServiceOptionId!.Value).Distinct().ToList();
            if (ids.Count == 0) return list;
            var names = await _db.ServiceOptions.Where(o => ids.Contains(o.Id)).ToDictionaryAsync(o => o.Id, o => o.Name);
            foreach (var r in list)
                if (r.ServiceOptionId != null && names.TryGetValue(r.ServiceOptionId.Value, out var n)) r.ServiceOptionName = n;
            return list;
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetAvailableRequestsForProvidersAsync(int? subCategoryId)
        {
            var requests = await _homeRepository.GetAvailableServiceRequestsAsync(subCategoryId);

            var list = requests.Select(r => new ServiceRequestDto
            {
                Id = r.Id,
                ServiceOptionId = r.ServiceOptionId,
                ClientName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "عميل برواح",
                ClientImage = r.User?.Images?.FirstOrDefault()?.Url ?? "",
                CategoryName = r.SubCategory?.Name ?? "", 
                Description = r.Description,
                Price = r.EstimatedPrice,
                PickupAddress = r.PickupAddress,
                DeliveryAddress = r.DeliveryAddress,
                PickupLatitude = r.PickupLatitude,
                PickupLongitude = r.PickupLongitude,
                DeliveryLatitude = r.DeliveryLatitude,
                DeliveryLongitude = r.DeliveryLongitude,
                AttachmentUrl = r.AttachmentUrl,
                TimeAgo = GetTimeAgo(r.CreatedAt)
            }).ToList();
            return await WithOptionNamesAsync(list);
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetAllServiceRequestsAsync()
        {
            var requests = await _homeRepository.GetAllServiceRequestsAsync();

            var list = requests.Select(r => new ServiceRequestDto
            {
                Id = r.Id,
                ServiceOptionId = r.ServiceOptionId,
                ClientName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "عميل برواح",
                ClientImage = r.User?.Images?.FirstOrDefault()?.Url ?? "",
                CategoryName = r.SubCategory?.Name ?? "",
                Description = r.Description,
                Price = r.EstimatedPrice,
                PickupAddress = r.PickupAddress,
                DeliveryAddress = r.DeliveryAddress,
                PickupLatitude = r.PickupLatitude,
                PickupLongitude = r.PickupLongitude,
                DeliveryLatitude = r.DeliveryLatitude,
                DeliveryLongitude = r.DeliveryLongitude,
                AttachmentUrl = r.AttachmentUrl,
                TimeAgo = GetTimeAgo(r.CreatedAt)
            }).ToList();
            return await WithOptionNamesAsync(list);
        }

        public async Task<ServiceOfferResponseDto> SubmitOfferAsync(CreateServiceOfferDto dto, int providerId)
        {
            string? attachmentUrl = null;

            if (dto.Attachment != null && dto.Attachment.Length > 0)
            {
                var uploadResult = await _imageService.UploadImageAsync(dto.Attachment);
                attachmentUrl = uploadResult.Url.ToString();
            }

            var offer = new ServiceOffer
            {
                ServiceRequestId = dto.ServiceRequestId,
                ProviderId = providerId,
                Price = dto.Price,
                Description = dto.Description,
                ExecutionPeriod = dto.ExecutionPeriod,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                AttachmentUrl = attachmentUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _homeRepository.AddAsync(offer);
            await _homeRepository.SaveChangesAsync();

            var ownerId = await _db.ServiceRequests.Where(r => r.Id == offer.ServiceRequestId).Select(r => (int?)r.UserId).FirstOrDefaultAsync();
            if (ownerId != null)
                await SafeNotifyAsync(ownerId.Value, "وصلك عرض جديد 💼", $"قدّم مقدم خدمة عرضًا بسعر {offer.Price:0.##} ج.م على طلبك #{offer.ServiceRequestId}. افتح طلباتي لمراجعته.");

            return MapToOfferResponseDto(offer);
        }

        private ServiceOfferResponseDto MapToOfferResponseDto(ServiceOffer offer)
        {
            return new ServiceOfferResponseDto
            {
                Id = offer.Id,
                ServiceRequestId = offer.ServiceRequestId,
                ProviderId = offer.ProviderId,
                Price = offer.Price,
                Description = offer.Description,
                ExecutionPeriod = offer.ExecutionPeriod,
                Latitude = offer.Latitude,
                Longitude = offer.Longitude,
                AttachmentUrl = offer.AttachmentUrl,
                CreatedAt = offer.CreatedAt
            };
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

    