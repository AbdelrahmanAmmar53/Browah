using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IAdvertisementRepository _advertisementRepository; 
        public CategoryService(ICategoryRepository categoryRepository, 
                               IMerchantRepository merchantRepository, 
                               IAdvertisementRepository advertisementRepository)
        {
            _categoryRepository = categoryRepository;
            _merchantRepository = merchantRepository;
            _advertisementRepository = advertisementRepository;
        }

        public async Task<CategoryContentResponseDto?> GetCategoryContentAsync(int categoryId, int page, int pageSize)
        {
            var category = await _categoryRepository.GetByIdWithSubCategoriesAsync(categoryId);
            if (category == null) return null;

            var response = new CategoryContentResponseDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                CategoryType = category.CategoryType.ToString(),

                SubCategories = category.SubCategories.Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name
                }).ToList()
            };

            List<AdvertisementResponseDto> advertisementDtos = new();

            switch (category.CategoryType)
            {
                case CategoryType.Restaurant:
                case CategoryType.Grocery:
                case CategoryType.HomeFood:
                    // Merchants are matched by the category's type, not by its name:
                    // the app registers them as "مطعم" / "بقالة" / "مطبخ بيتي", while
                    // the categories are named "مطاعم" / "البقالة" / "اكل بيتي".
                    var types = category.CategoryType switch
                    {
                        CategoryType.Restaurant => new[] { "مطعم", "مطاعم", "restaurant" },
                        CategoryType.Grocery => new[] { "بقالة", "البقالة", "grocery" },
                        _ => new[] { "مطبخ بيتي", "اكل بيتي", "أكل بيتي", "homefood" }
                    };
                    // A store registered as "other" (any free-text activity) is shown as a restaurant,
                    // so the restaurant list is everyone who is not grocery / home food.
                    var groceryTypes = new[] { "بقالة", "البقالة", "grocery" };
                    var homeTypes = new[] { "مطبخ بيتي", "اكل بيتي", "أكل بيتي", "homefood" };
                    var merchants = category.CategoryType == CategoryType.Restaurant
                        ? await _merchantRepository.GetMerchantsExceptBusinessTypesAsync(groceryTypes.Concat(homeTypes).ToArray())
                        : await _merchantRepository.GetMerchantsByBusinessTypesAsync(types);
                    response.Items = merchants.Select(m => new MerchantCardDto
                    {
                        MerchantId = m.Id,
                        BusinessName = m.BusinessName ?? $"{m.FirstName} {m.LastName}".Trim(),
                        BusinessDescription = m.BusinessDescription ?? string.Empty,
                        BusinessType = m.BusinessType ?? string.Empty,
                        MerchantImageUrl = m.ProfileImageUrl ?? m.Images?.FirstOrDefault()?.Url,
                        Address = m.Address ?? string.Empty,
                        // Closed stores are listed too, flagged closed (the merchant toggles IsFrozen).
                        IsOpen = !m.IsFrozen && !m.IsPaused,
                        WorkHours = m.WorkHours,
                        Rating = m.Rating > 0 ? (decimal?)m.Rating : null,
                        SubCategoryIds = ParseIds(m.BusinessSubCategoryIds)
                    }).Cast<object>().ToList();
                    break;

                case CategoryType.Standard:
                case CategoryType.Services:
                default:

                    if (category.Name.Contains("سوق برواح"))
                    {
                        var adminAds = await _advertisementRepository.GetAdminAdvertisementsAsync(page, pageSize);
                        advertisementDtos = adminAds.Select(MapToResponseDto).ToList();
                    }
                    
                    else if (category.SubCategories != null && category.SubCategories.Any())
                    {
                        var subCategoryIds = category.SubCategories.Select(sc => sc.Id).ToList();
                        var subCategoryAds = await _advertisementRepository.GetBySubCategoryIdsAsync(subCategoryIds, page, pageSize);
                        advertisementDtos = subCategoryAds.Select(MapToResponseDto).ToList();
                    }

                    else
                    {
                        var ads = await _advertisementRepository.GetByCategoryIdAsync(categoryId, page, pageSize);
                        advertisementDtos = ads.Select(MapToResponseDto).ToList();
                    }

                    response.Items = advertisementDtos.Cast<object>().ToList();
                    break;
            }

            return response;
        }

        public async Task<IEnumerable<AdvertisementResponseDto>> GetFilteredAdsAsync(int categoryId, int? subCategoryId, string? condition, int page, int pageSize)
        {

            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null) return Enumerable.Empty<AdvertisementResponseDto>();

            IEnumerable<Advertisment> ads;

   
            if (category.Name.Contains("السوق العام") || category.Name.Contains("سوق عام"))
            {
                ads = await _categoryRepository.GetByConditionAsync(categoryId, condition, page, pageSize);
            }
            else
            {

                if (subCategoryId.HasValue && subCategoryId.Value > 0)
                {
                    ads = await _categoryRepository.GetBySubCategoryIdAsync(subCategoryId.Value, page, pageSize);
                }
                else
                {
                    ads = await _categoryRepository.GetByCategoryIdAsync(categoryId, page, pageSize);
                }
            }

            return ads.Select(MapToResponseDto).ToList();
        }

        private static List<int> ParseIds(string? csv) =>
            string.IsNullOrWhiteSpace(csv)
                ? new List<int>()
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                     .Select(x => int.TryParse(x, out var n) ? n : 0).Where(n => n > 0).Distinct().ToList();

        private AdvertisementResponseDto MapToResponseDto(Advertisment ad)
        {
            return new AdvertisementResponseDto
            {
                Id = ad.Id,
                Title = ad.Title,
                Description = ad.Description,
                Price = ad.Price,
                IsNegotiable = ad.IsNegotiable,
                Condition = ad.Condition.ToString(),
                Governorate = ad.Governorate,
                City = ad.City,
                CategoryId = ad.CategoryId,
                SubCategoryId = ad.SubCategoryId, 
                PromotionPlan = ad.PromotionPlan.ToString(),
                ImageUrls = ad.AdvertismentImages?.Select(ai => ai.Image!.Url).ToList() ?? new(),
                Specifications = ad.Specifications?.Select(s => new SpecificationItemDto
                {
                    Id = s.Id,
                    Key = s.SpecificationKey,
                    Value = s.SpecificationValue
                }).ToList() ?? new(),
                CreatedAt = ad.CreatedAt
            };
        }
    }
}

