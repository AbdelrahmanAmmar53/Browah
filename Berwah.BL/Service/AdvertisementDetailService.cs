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
    public class AdvertisementDetailService : IAdvertisementDetailService
    {
        private readonly IAdvertisementRepository _adRepository;
        private readonly ApplicationDbContext _context;

        public AdvertisementDetailService(IAdvertisementRepository adRepository, ApplicationDbContext context)
        {
            _adRepository = adRepository;
            _context = context;
        }

        public async Task<AdvertisementDetailDto?> GetAdvertisementDetailAsync(int advertisementId)
        {
            var ad = await _adRepository.GetByIdAsync(advertisementId);
            if (ad == null) return null;

            var specifications = await GetSpecificationsAsync(advertisementId);

            return new AdvertisementDetailDto
            {
                Id = ad.Id,
                Title = ad.Title,
                Description = ad.Description,
                Price = ad.Price,
                IsNegotiable = ad.IsNegotiable,
                Condition = ad.Condition.ToString(),
                CreatedAt = ad.CreatedAt,
                Governorate = ad.Governorate,
                City = ad.City,
                PromotionPlan = ad.EffectivePlan.ToString(),
                ImageUrl = ad.AdvertismentImages.Select(ai => ai.Image?.Url ?? "").FirstOrDefault() ?? "",
                CategoryId = ad.CategoryId,
                CategoryName = ad.Category?.Name ?? "",
                UserId = ad.UserId,
                SellerName = $"{ad.User?.FirstName} {ad.User?.LastName}".Trim(),
                SellerPhone = ad.User?.PhoneNumber ?? "",
                SellerImage = ad.User?.Images?.FirstOrDefault()?.Url ?? "",
                IsSellerVerified = true,
                Specifications = specifications
            };
        }

        public async Task<List<SpecificationItemDto>> GetSpecificationsAsync(int advertisementId)
        {
            var specs = await _adRepository.GetSpecificationsByAdvertisementIdAsync(advertisementId);
            return specs.Select(s => new SpecificationItemDto
            {
                Id = s.Id,
                Key = s.SpecificationKey,
                Value = s.SpecificationValue
            }).ToList();
        }

        public async Task<SellerProfileDto?> GetSellerProfileAsync(int sellerId)
        {
            var user = await _context.Users
                .Include(u => u.Images)
                .FirstOrDefaultAsync(u => u.Id == sellerId);

            if (user == null) return null;

            // احسب الإحصائيات الأساسية فقط
            var totalAds = await _adRepository.GetAdvertisementCountByUserIdAsync(sellerId);
            var activeAds = await _context.Advertisements
                .Where(a => a.UserId == sellerId && a.Status == Berwah.Models.Enum.AdStatus.Approved)
                .CountAsync();

            var recentAds = await _context.Advertisements
                .Where(a => a.UserId == sellerId && a.Status == Berwah.Models.Enum.AdStatus.Approved)
                .Include(a => a.AdvertismentImages)
                    .ThenInclude(ai => ai.Image)
                .OrderByDescending(a => a.CreatedAt)
                .Take(6)
                .Select(a => new AdvertisementCardDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Price = a.Price,
                    ImageUrl = a.AdvertismentImages
                        .OrderBy(ai => ai.ImageId)
                        .Select(ai => ai.Image != null ? ai.Image.Url : "")
                        .FirstOrDefault() ?? "",
                    City = a.City,
                    Governorate = a.Governorate,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return new SellerProfileDto
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                ProfileImage = user.Images?.FirstOrDefault()?.Url ?? "",
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Governorate = "Egypt",
                City = user.Address ?? "Unknown",
                TotalAds = totalAds,
                ActiveAds = activeAds,
                MemberSince = user.CreatedAt,
                IsVerified = true,
                VerificationStatus = "معتمد",
                RecentAdvertisements = recentAds
            };
        }

        public async Task AddSpecificationsAsync(AdvertisementSpecificationDto dto)
        {
            var ad = await _adRepository.GetByIdAsync(dto.AdvertisementId);
            if (ad == null)
                throw new Exception("الإعلان غير موجود");

            foreach (var specItem in dto.Specifications)
            {
                var specification = new AdvertisementSpecification
                {
                    AdvertisementId = dto.AdvertisementId,
                    SpecificationKey = specItem.Key,
                    SpecificationValue = specItem.Value,
                    CreatedAt = DateTime.UtcNow
                };

                await _adRepository.AddSpecificationAsync(specification);
            }

            await _adRepository.SaveChangesAsync();
        }

        public async Task DeleteSpecificationAsync(int specificationId)
        {
            await _adRepository.DeleteSpecificationAsync(specificationId);
        }
    }
}