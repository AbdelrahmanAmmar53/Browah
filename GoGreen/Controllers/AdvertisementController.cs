using Berwah.BL.Service.IService;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Berwah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvertisementController : ControllerBase
    {
        private readonly IAdvertismentService _advertisementService;
        private readonly IAdvertisementDetailService _advertisementDetailService;
        private readonly AdPromotionService _promotions;

        public AdvertisementController(
            IAdvertismentService advertisementService,
            IAdvertisementDetailService advertisementDetailService,
            AdPromotionService promotions)
        {
            _advertisementService = advertisementService;
            _advertisementDetailService = advertisementDetailService;
            _promotions = promotions;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAdvertisement([FromForm] CreateAdvertisementDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { status = "fail", message = "غير مصرح لك بالوصول" });
            }

            // Featured / VIP are for regular users and need a payment method; checked before anything is saved.
            var paid = dto.PromotionPlan != PromotionPlanType.Free;
            if (paid)
            {
                if (User.IsInRole("Merchant"))
                    return StatusCode(403, new { status = "fail", message = "باقات الإعلانات متاحة للمستخدمين العاديين فقط" });
                var invalid = AdPromotionService.Validate(dto.PromotionPlan, dto.PaymentMethod, dto.PaymentReference);
                if (invalid != null)
                    return BadRequest(new { status = "fail", message = invalid });
            }

            try
            {
                var result = await _advertisementService.CreateAdvertisementAsync(dto, userId);
                if (paid)
                    await _promotions.CreatePurchaseAsync(result.Id, userId, dto.PromotionPlan, dto.PaymentMethod!.Value, dto.PaymentReference);
                return CreatedAtAction(nameof(GetAdvertisementById), new { id = result.Id }, new { status = "success", data = result, promotionPending = paid });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new
                {
                    status = "error",
                    message = "حدث خطأ أثناء حفظ الإعلان",
                    details = innerMessage
                });
            }
        }

        [Authorize]
        [HttpGet("my-advertisements")]
        public async Task<IActionResult> GetMyAdvertisements(CancellationToken ct = default)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { status = "fail", message = "غير مصرح لك بالوصول" });
            }

            try
            {
                
                var ads = await _advertisementService.GetUserAdvertisementsAsync(userId); 
                return Ok(new
                {
                    status = "success",
                    data = ads
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = "حدث خطأ أثناء جلب الإعلانات",
                    details = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdvertisementById(int id)
        {
            var advertisement = await _advertisementService.GetByIdAsync(id);
            if (advertisement == null)
            {
                return NotFound(new { status = "fail", message = "الإعلان غير موجود" });
            }
            return Ok(new { status = "success", data = advertisement });
        }

        [Authorize]
        [HttpDelete("DeleteAdvertisement/{id}")]
        public async Task<IActionResult> DeleteAdvertisement(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { status = "fail", message = "غير مصرح لك بالوصول" });
            }

            try
            {
                var deleted = await _advertisementService.DeleteAdvertisementAsync(id, userId);
                if (!deleted)
                {
                    return NotFound(new { status = "fail", message = "الإعلان غير موجود أو ليس لديك صلاحية لحذفه" });
                }

                return Ok(new { status = "success", message = "تم حذف الإعلان وجميع المواصفات المرتبطة به بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = "حدث خطأ أثناء حذف الإعلان",
                    details = ex.Message
                });
            }
        }

        [HttpGet("GetSpecifications/{advertisementId}")]
        public async Task<IActionResult> GetSpecifications(int advertisementId)
        {
            var specs = await _advertisementDetailService.GetSpecificationsAsync(advertisementId);
            return Ok(new { status = "success", data = specs });
        }

        [HttpGet("GetSellerProfile/{sellerId}")]
        public async Task<IActionResult> GetSellerProfile(int sellerId)
        {
            var profile = await _advertisementDetailService.GetSellerProfileAsync(sellerId);
            if (profile == null)
                return NotFound(new { status = "fail", message = "البائع غير موجود" });

            return Ok(new { status = "success", data = profile });
        }

        [Authorize]
        [HttpPost("AddSpecifications")]
        public async Task<IActionResult> AddSpecifications([FromBody] AdvertisementSpecificationDto dto)
        {
            try
            {
                await _advertisementDetailService.AddSpecificationsAsync(dto);
                return Ok(new { status = "success", message = "تمت إضافة المواصفات بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "fail", message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("DeleteSpecification/{specificationId}")]
        public async Task<IActionResult> DeleteSpecification(int specificationId)
        {
            try
            {
                await _advertisementDetailService.DeleteSpecificationAsync(specificationId);
                return Ok(new { status = "success", message = "تم حذف الخاصية بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "fail", message = ex.Message });
            }
        }
        [Authorize]
        [HttpGet("general-feed")]
        public async Task<IActionResult> GetGeneralFeed()
        {
            var ads = await _advertisementService.GetAllFeedAsync();
            return Ok(new { status = "success", data = ads });
        }

        [Authorize]
        [HttpGet("browah-market")]
        public async Task<IActionResult> GetBrowahMarket()
        {
            var ads = await _advertisementService.GetBrowahMarketFeedAsync();
            return Ok(new
            {
                status = "success",
                data = ads
            });
        }
    }
}
    