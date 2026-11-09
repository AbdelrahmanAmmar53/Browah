using Berwah.BL.Service.IService;
using Berwah.Models.DTO;
using Berwah.Models.DTO.SupervisorForAd;
using Berwah.Models.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SupervisorForAd,SupervisorUsed,BossOfSupervisor,Admin")]
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

        // 4. إعلانات بأي حالة (Pending / Approved / Rejected / Suspended)
        [HttpGet("ads")]
        public async Task<IActionResult> GetAdsByStatus([FromQuery] string status = "Pending", [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (!Enum.TryParse<AdStatus>(status, true, out var parsed))
                return BadRequest(new { status = "error", message = "حالة غير معروفة" });
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 50;
            var result = await _supervisorService.GetAdsByStatusAsync(parsed, page, pageSize);
            return Ok(new { status = "success", data = result });
        }

        // 5. تفاصيل إعلان
        [HttpGet("ad/{id}")]
        public async Task<IActionResult> GetAd(int id)
        {
            var ad = await _supervisorService.GetAdDetailsAsync(id);
            if (ad == null) return NotFound(new { status = "error", message = "الإعلان غير موجود" });
            return Ok(new { status = "success", data = ad });
        }

        // 6. تعديل بيانات الإعلان من المشرف
        [HttpPut("ad/{id}")]
        public async Task<IActionResult> UpdateAd(int id, [FromBody] UpdateDetailAdDto dto)
        {
            var ok = await _supervisorService.UpdateAdDetailsAsync(id, dto);
            if (!ok) return BadRequest(new { status = "error", message = "تعذر تعديل الإعلان" });
            return Ok(new { status = "success", message = "تم تعديل الإعلان" });
        }

        // 7. رفض مع سبب (يصل لصاحب الإعلان في إشعار)
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectAdDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.RejectReason))
                return BadRequest(new { status = "error", message = "سبب الرفض مطلوب" });
            var ok = await _supervisorService.RejectAdWithReasonAsync(id, dto);
            if (!ok) return BadRequest(new { status = "error", message = "تعذر رفض الإعلان" });
            return Ok(new { status = "success", message = "تم رفض الإعلان" });
        }

        // 8. إخفاء / تعليق مع رسالة لصاحب الإعلان
        [HttpPost("hide/{id}")]
        public async Task<IActionResult> Hide(int id, [FromBody] HideAdDto dto)
        {
            var ok = await _supervisorService.HideAdAsync(id, dto);
            if (!ok) return BadRequest(new { status = "error", message = "تعذر تعليق الإعلان" });
            return Ok(new { status = "success", message = "تم تعليق الإعلان" });
        }

        // 9. تقارير المراجعة (today / week / month / all)
        [HttpGet("reports")]
        public async Task<IActionResult> Reports([FromQuery] string period = "week")
        {
            var report = await _supervisorService.GetReportsAsync(period);
            return Ok(new { status = "success", data = report });
        }
    }
}
