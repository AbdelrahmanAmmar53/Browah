using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class AdvertisementService: IAdvertismentService
    {
        private readonly IAdvertisementRepository _adRepository;
        private readonly IImageService _imageService; // خدمة رفع الصور (مثلاً Cloudinary)

        public AdvertisementService(IAdvertisementRepository adRepository, IImageService imageService)
        {
            _adRepository = adRepository;
            _imageService = imageService;
        }

        public async Task<AdvertisementResponseDto> CreateAdvertisementAsync(CreateAdvertisementDto dto, int userId)
        {
            // 1. تحويل الـ DTO إلى Entity الإعلان
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
                //SubCategoryId = dto.SubCategoryId,
                PromotionPlan = dto.PromotionPlan,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,

                AdvertismentImages = new List<AdvertismentImage>()
            };

            // 2. معالجة الصور المرفوعة وربطها عبر AdvertisementImage
            if (dto.Images != null && dto.Images.Count > 0)
            {
                for (int i = 0; i < dto.Images.Count; i++)
                {
                    // رفع الصورة للحصول على الـ Url والـ PublicId
                    var uploadResult = await _imageService.UploadImageAsync(dto.Images[i]);

                    var imageEntity = new Image
                    {
                        Url = uploadResult.Url.ToString(),
                        PublicId = uploadResult.PublicId,
                        FileName = dto.Images[i].FileName,
                        FileType = dto.Images[i].ContentType,
                        UserId = userId
                    };

                    // الربط المباشر مع جدول الوسيط
                    ad.AdvertismentImages.Add(new AdvertismentImage
                    {
                        Image = imageEntity,
                       // IsMain = (i == dto.MainImageIndex) 
                    });
                }
            }

            await _adRepository.AddAsync(ad);
            await _adRepository.SaveChangesAsync();

            return new AdvertisementResponseDto
            {
                Id = ad.Id,
                Title = ad.Title,
                Price = ad.Price,
                IsNegotiable = ad.IsNegotiable,
                Condition = ad.Condition.ToString(),
                Governorate = ad.Governorate,
                City = ad.City,
                PromotionPlan = ad.PromotionPlan.ToString(),
                ImageUrls = ad.AdvertismentImages.Select(ai => ai.Image!.Url).ToList(),
                CreatedAt = ad.CreatedAt
            };
        }

        public async Task<AdvertisementResponseDto?> GetByIdAsync(int id)
        {
            var ad = await _adRepository.GetByIdAsync(id);
            if (ad == null) return null;

            return new AdvertisementResponseDto
            {
                Id = ad.Id,
                Title = ad.Title,
                Price = ad.Price,
                IsNegotiable = ad.IsNegotiable,
                Condition = ad.Condition.ToString(),
                Governorate = ad.Governorate,
                City = ad.City,
                PromotionPlan = ad.PromotionPlan.ToString(),
                ImageUrls = ad.AdvertismentImages.Select(ai => ai.Image!.Url).ToList(),
                CreatedAt = ad.CreatedAt
            };
        }
    }
}