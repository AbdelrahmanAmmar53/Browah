using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class AdvertisementService: IAdvertismentService
    {
        private readonly IAdvertisementRepository _adRepository;
        private readonly IImageService _imageService; 

        public AdvertisementService(IAdvertisementRepository adRepository, IImageService imageService)
        {
            _adRepository = adRepository;
            _imageService = imageService;
        }

        public async Task<AdvertisementResponseDto> CreateAdvertisementAsync(CreateAdvertisementDto dto, int userId)
        {
            var ad = new Advertisment
            {
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                Price = dto.Price,
                IsNegotiable = dto.IsNegotiable,
                Condition = dto.Condition,
                Governorate = dto.Governorate,
                City = dto.City,
                CategoryId = dto.CategoryId,
                PromotionPlan = dto.PromotionPlan,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                AdvertismentImages = new List<AdvertismentImage>(),
                Specifications = new List<AdvertisementSpecification>()
            };

            if (dto.Images != null && dto.Images.Count > 0)
            {
                for (int i = 0; i < dto.Images.Count; i++)
                {
                    var uploadResult = await _imageService.UploadImageAsync(dto.Images[i]);
                    var imageEntity = new Image
                    {
                        Url = uploadResult.Url.ToString(),
                        PublicId = uploadResult.PublicId,
                        FileName = dto.Images[i].FileName,
                        FileType = dto.Images[i].ContentType,
                        UserId = userId
                    };

                    ad.AdvertismentImages.Add(new AdvertismentImage
                    {
                        Image = imageEntity
                    });
                }
            }

            await _adRepository.AddAsync(ad);
            await _adRepository.SaveChangesAsync();

       
            if (!string.IsNullOrEmpty(dto.Key))
            {
                var spec = new AdvertisementSpecification
                {
                    AdvertisementId = ad.Id,
                    SpecificationKey = dto.Key,
                    SpecificationValue = dto.Value ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                await _adRepository.AddSpecificationAsync(spec);
                await _adRepository.SaveChangesAsync();

               
                ad.Specifications.Add(spec);
            }

            return MapToResponseDto(ad);
        }
        public async Task<AdvertisementResponseDto?> GetByIdAsync(int id)
        {
            var ad = await _adRepository.GetByIdAsync(id);
            if (ad == null) return null;

            return MapToResponseDto(ad);
        }

        public async Task<bool> DeleteAdvertisementAsync(int advertisementId, int userId)
        {

            var ad = await _adRepository.GetByIdAsync(advertisementId);
            if (ad == null) return false;


            if (ad.UserId != userId)
            {
                return false;
            }


            await _adRepository.DeleteAsync(ad);
            return true;
        }

        public async Task<IEnumerable<AdvertisementResponseDto>> GetUserAdvertisementsAsync(int userId)
        {
            var ads = await _adRepository.GetByUserIdAsync(userId);
            return ads.Select(MapToResponseDto).ToList();
        }

        public async Task<IEnumerable<AdvertisementResponseDto>> GetAllFeedAsync()
        {
            var ads = await _adRepository.GetAllAsync();
            return ads.Select(MapToResponseDto).ToList();
        }

        public async Task<IEnumerable<AdvertisementResponseDto>> GetBrowahMarketFeedAsync()
        {

            var ads = await _adRepository.GetBrowahMarketFeedAsync();
            return ads.Select(MapToResponseDto).ToList();
        }

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
                PromotionPlan = ad.PromotionPlan.ToString(),
                ImageUrls = ad.AdvertismentImages.Select(ai => ai.Image!.Url).ToList(),

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