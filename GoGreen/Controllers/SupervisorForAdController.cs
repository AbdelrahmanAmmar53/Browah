using Berwah.BL.Service.IService;
using Berwah.Models.DTO;
using Berwah.Models.DTO.SupervisorForAd;
using Berwah.Models.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupervisorForAdController : ControllerBase
    {
        private readonly ISupervisorForAdService _supervisorService;

        public SupervisorForAdController(ISupervisorForAdService supervisorService)
        {
            _supervisorService = supervisorService;
        }

        // 1. جلب الإعلانات التي في انتظار المراجعة (Pending)
        [HttpGet("pending-ads")]
        public async Task<IActionResult> GetPendingAds([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _supervisorService.GetAdsByStatusAsync(AdStatus.Pending, page, pageSize);
            return Ok(new { status = "success", data = result });
        }

        // 2. إحصائيات المشرف (عدد المعلق والمقبول) كما في التصميم العلوي للشاشة
        [HttpGet("stats")]
        public async Task<IActionResult> GetSupervisorStats()
        {
            var stats = await _supervisorService.GetAdsStatsAsync();
            return Ok(new { status = "success", data = stats });
        }

        // 3. تغيير حالة الإعلان (قبول / رفض / تعليق)
        [HttpPost("update-ad-status/{id}")]
        public async Task<IActionResult> UpdateAdStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var success = await _supervisorService.UpdateAdStatusAsync(id, dto.status);
            if (!success)
                return BadRequest(new { status = "error", message = "فشل تحديث حالة الإعلان أو الإعلان غير موجود" });

            return Ok(new { status = "success", message = "تم تحديث حالة الإعلان بنجاح" });
        }

        // 4. جلب تفاصيل إعلان معين كاملة (شاشة تفاصيل الإعلان والتعديل)
        [HttpGet("ad-details/{id}")]
        public async Task<IActionResult> GetAdDetails(int id)
        {
            var ad = await _supervisorService.GetAdDetailsAsync(id);
            if (ad == null)
                return NotFound(new { status = "error", message = "الإعلان غير موجود" });

            return Ok(new { status = "success", data = ad });
        }

        // 5. تعديل بيانات الإعلان من قبل المشرف
        [HttpPut("update-ad/{id}")]
        public async Task<IActionResult> UpdateAdDetails(int id, [FromBody] UpdateDetailAdDto dto)
        {
            var success = await _supervisorService.UpdateAdDetailsAsync(id, dto);
            if (!success)
                return BadRequest(new { status = "error", message = "فشل تعديل الإعلان أو الإعلان غير موجود" });

            return Ok(new { status = "success", message = "تم تعديل تفاصيل الإعلان بنجاح" });
        }

        // 6. رفض الإعلان مع الأسباب (نافذة سبب الرفض)
        [HttpPost("reject-ad/{id}")]
        public async Task<IActionResult> RejectAd(int id, [FromBody] RejectAdDto dto)
        {
            var success = await _supervisorService.RejectAdWithReasonAsync(id, dto);
            if (!success)
                return BadRequest(new { status = "error", message = "فشل رفض الإعلان أو الإعلان غير موجود" });

            return Ok(new { status = "success", message = "تم رفض الإعلان وإرسال سبب الرفض للمعلن" });
        }

        // 7. إخفاء / تعليق الإعلان (صفحة إخفاء الإعلان وإخطار الناشر)
        [HttpPost("hide-ad/{id}")]
        public async Task<IActionResult> HideAd(int id, [FromBody] HideAdDto dto)
        {
            var success = await _supervisorService.HideAdAsync(id, dto);
            if (!success)
                return BadRequest(new { status = "error", message = "فشل إخفاء الإعلان أو الإعلان غير موجود" });

            return Ok(new { status = "success", message = "تم تعليق الإعلان وإرسال الإخطار للمعلن" });
        }

        // 8. جلب تقارير قسم المنتجات الجديدة (اليوم، الأسبوع، الشهر، مخصص)
        [HttpGet("reports")]
        public async Task<IActionResult> GetReports([FromQuery] string period = "week")
        {
            try
            {
                var report = await _supervisorService.GetReportsAsync(period);
                return Ok(new { status = "success", data = report });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // 9. تعديل وتنفيذ الإجراء الرقابي المتقدم (نافذة تعديل الحالة الرقابية)
        [HttpPost("regulatory-action/{id}")]
        public async Task<IActionResult> ExecuteRegulatoryAction(int id, [FromBody] RegulatoryActionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var success = await _supervisorService.ExecuteRegulatoryActionAsync(id, dto);
                if (!success)
                    return NotFound(new { status = "error", message = "الإعلان غير موجود أو فشل التحديث" });

                return Ok(new { status = "success", message = "تم حفظ وتحديث الحالة الرقابة للإعلان بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}
