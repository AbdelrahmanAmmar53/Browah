using Berwah.BL.Service.IService;
using Berwah.Models.DTO;
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

        public AdvertisementController(
            IAdvertismentService advertisementService,
            IAdvertisementDetailService advertisementDetailService)
        {
            _advertisementService = advertisementService;
            _advertisementDetailService = advertisementDetailService;
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

            try
            {
                var result = await _advertisementService.CreateAdvertisementAsync(dto, userId);
                return CreatedAtAction(nameof(GetAdvertisementById), new { id = result.Id }, new { status = "success", data = result });
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
    }
}
    