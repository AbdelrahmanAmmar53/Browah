using Berwah.BL.Service.IService;
using Berwah.Models.DTO;
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
    }
}
