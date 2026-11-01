using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class SupervisorForAdService : ISupervisorForAdService
    {
        private readonly ISupervisorForAdRepository _supervisorRepo;

        public SupervisorForAdService(ISupervisorForAdRepository     supervisorRepo)
        {
            _supervisorRepo = supervisorRepo;
        }

        public async Task<IEnumerable<AdDetailsResponseDto>> GetAdsByStatusAsync(AdStatus status, int page, int pageSize)
        {
            var ads = await _supervisorRepo.GetAdsByStatusAsync(status, page, pageSize);

            return ads.Select(a => new AdDetailsResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Price = a.Price,
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt,
                SellerName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "مستخدم",
                ImageUrls = a.AdvertismentImages?.Select(ai => ai.Image?.Url ?? "").ToList() ?? new()
            });
        }

        public async Task<SupervisorAdStatusDto> GetAdsStatsAsync()
        {
            var pendingCount = await _supervisorRepo.GetAdsCountByStatusAsync(AdStatus.Pending);
            var approvedCount = await _supervisorRepo.GetAdsCountByStatusAsync(AdStatus.Approved);

            return new SupervisorAdStatusDto
            {
                PendingCount = pendingCount,
                ApprovedCount = approvedCount
            };
        }

        public async Task<bool> UpdateAdStatusAsync(int adId, AdStatus status)
        {
            var ad = await _supervisorRepo.GetByIdAsync(adId);
            if (ad == null) return false;

            ad.Status = status;
            return await _supervisorRepo.UpdateAsync(ad);
        }
    }
}

