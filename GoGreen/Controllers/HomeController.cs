using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Service;
using Berwah.Models.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;
        private readonly ApplicationDbContext _db;
        public HomeController(IHomeService homeService, ApplicationDbContext db)
        {
            _homeService = homeService;
            _db = db;
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
            // 🟢 تعديل: جلب الطلبات المعتمدة من المشرف فقط
            var requests = (await _homeService.GetAvailableRequestsForProvidersAsync(subCategoryId))
                .Where(r => r.Status == ServiceRequestStatus.Approved)
                .ToList();

            await MarkMyOffersAsync(requests);
            return Ok(new { status = "success", data = requests });
        }

        [HttpGet("all-service-requests")]
        public async Task<IActionResult> GetAllServiceRequests()
        {
            // 🟢 تعديل: جلب الطلبات المعتمدة من المشرف فقط
            var requests = (await _homeService.GetAllServiceRequestsAsync())
                .Where(r => r.Status == ServiceRequestStatus.Approved)
                .ToList();

            await MarkMyOffersAsync(requests);
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

            if (await _db.ServiceOffers.AnyAsync(o => o.ServiceRequestId == dto.ServiceRequestId && o.ProviderId == providerId))
                return BadRequest(new { status = "error", message = "قدمت عرضًا على هذا الطلب من قبل" });

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

        /// <summary>Marks the requests this account already offered on, so a refresh keeps showing it.</summary>
        private async Task MarkMyOffersAsync(IList<Berwah.Models.DTO.Service.ServiceRequestDto> requests)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var me) || requests.Count == 0) return;
            var ids = requests.Select(r => r.Id).ToList();

            // My own requests are not opportunities for me: leave them out of the list.
            var ownIds = (await _db.ServiceRequests.AsNoTracking()
                .Where(r => r.UserId == me && ids.Contains(r.Id))
                .Select(r => r.Id).ToListAsync()).ToHashSet();
            for (var i = requests.Count - 1; i >= 0; i--)
                if (ownIds.Contains(requests[i].Id)) requests.RemoveAt(i);
            if (requests.Count == 0) return;
            ids = requests.Select(r => r.Id).ToList();

            var mine = await _db.ServiceOffers.AsNoTracking()
                .Where(o => o.ProviderId == me && ids.Contains(o.ServiceRequestId))
                .Select(o => o.ServiceRequestId).Distinct().ToListAsync();
            var set = mine.ToHashSet();
            foreach (var r in requests) r.HasMyOffer = set.Contains(r.Id);
        }
    }
}

