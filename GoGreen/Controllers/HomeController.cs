using Berwah.BL.Service.IService;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;
        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        [Authorize]
        [HttpGet("GetHomeData")]
        public async Task<IActionResult> GetHomeData()
        {
           
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var result = await _homeService.GetHomeDataAsync(userId);
            return Ok(result);
        }

        [HttpGet("GetWebHomeData")]
        public async Task<IActionResult> GetWebHomeData()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _homeService.GetWebHomeDataAsync(userId);
            return Ok(new { status = "success", data = result });
        }

        //[HttpGet("services-page")]
        //public async Task<IActionResult> GetServicesPage()
        //{
        //    try
        //    {
        //        var services = await _homeService.GetServicesPageDataAsync();
        //        return Ok(new
        //        {
        //            status = "success",
        //            data = services
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            status = "error",
        //            message = "حدث خطأ أثناء جلب بيانات الخدمات",
        //            details = ex.Message
        //        });
        //    }
        //}

        //[HttpGet("homemade-food")]
        //public async Task<IActionResult> GetHomeMadeFoodMerchants()
        //{
        //    var result = await _homeService.GetHomeMadeFoodAsync();
        //    return Ok(new { status = "success", data = result });
        //}

        [Authorize]
        [HttpPost("create-service-request")]
        public async Task<IActionResult> CreateServiceRequest([FromForm] CreateServiceRequest dto)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { status = "error", message = "يجب تسجيل الدخول أولاً أو أن المعرف غير صالح" });
            }

            var success = await _homeService.CreateServiceRequestAsync(userId, dto);
            if (!success)
                return BadRequest(new { status = "error", message = "فشل في إرسال الطلب" });

            return Ok(new { status = "success", message = "تم إرسال طلبك مع المرفق بنجاح" });
        }

        
        [HttpGet("available-service-requests")]
        public async Task<IActionResult> GetAvailableServiceRequests([FromQuery] int? subCategoryId)
        {
            var requests = await _homeService.GetAvailableRequestsForProvidersAsync(subCategoryId);
            return Ok(new { status = "success", data = requests });
        }

       
        [HttpGet("all-service-requests")]
        public async Task<IActionResult> GetAllServiceRequests()
        {
            var requests = await _homeService.GetAllServiceRequestsAsync();
            return Ok(new { status = "success", data = requests });
        }

        
        [HttpPost("submit-offer")]
        public async Task<IActionResult> SubmitServiceOffer([FromForm] CreateServiceOfferDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var providerId))
            {
                return Unauthorized(new { status = "fail", message = "غير مصرح لك بالوصول" });
            }

            try
            {
                var result = await _homeService.SubmitOfferAsync(dto, providerId);
                return Ok(new
                {
                    status = "success",
                    message = "تم إرسال عرضك للعميل بنجاح",
                    data = result
                });
            }
            catch (Exception ex)
            {
                // 🟢 هذا السطر سيجلب لك السبب الحقيقي الدقيق من قاعدة البيانات
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                return BadRequest(new
                {
                    status = "error",
                    message = "حدث خطأ أثناء حفظ العرض",
                    details = innerMessage
                });
            }
        }
    }
}

