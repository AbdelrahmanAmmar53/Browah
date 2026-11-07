using Berwah.BL.Service.IService;
using Berwah.Models.DTO.SupervisorForAd;
using Berwah.Models.DTO.SupervisorForSR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupervisorForSRController : ControllerBase
    {
        private readonly ISupervisorForSRService _service;
        private readonly ISupervisorForAdService _supervisorService;

        public SupervisorForSRController(ISupervisorForSRService service, ISupervisorForAdService supervisorService)
        {
            _service = service;
            _supervisorService = supervisorService;
        }

        // 1. جلب طلبات الخدمات قيد المراجعة
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetPendingRequestsAsync(page, pageSize);
            return Ok(new { status = "success", data = result });
        }

        // 2. إحصائيات لوحة التحكم العلوية
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _service.GetStatsAsync();
            return Ok(new { status = "success", data = stats });
        }

        // 3. اعتماد ونشر طلب الخدمة (إجراء إشرافي معتمد)
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveServiceRequestDto dto)
        {
            var success = await _service.ApproveRequestAsync(id, dto);
            if (!success) return NotFound(new { status = "error", message = "الطلب غير موجود" });

            return Ok(new { status = "success", message = "تم اعتماد ونشر طلب الخدمة بنجاح" });
        }

        // 4. رفض طلب الخدمة
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectServiceRequestDto dto)
        {
            var success = await _service.RejectRequestAsync(id, dto);
            if (!success)
                return NotFound(new { status = "error", message = "الطلب غير موجود" });

            return Ok(new { status = "success", message = "تم رفض الطلب وتسجيل الإجراء في السجل الرقابي بنجاح" });
        }

        // 5. معاينة وتفاصيل طلب خدمة معين (الشاشة الثالثة)
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetRequestDetails(int id)
        {
            var result = await _service.GetRequestDetailsAsync(id);
            if (result == null)
                return NotFound(new { status = "error", message = "الطلب غير موجود" });

            return Ok(new { status = "success", data = result });
        }

        // 6. تحديث وتعديل التصنيف أو السعر التقديري مع الاعتماد الفوري
        [HttpPost("update-and-approve/{id}")]
        public async Task<IActionResult> UpdateAndApprove(int id, [FromBody] SupervisorUpdateServiceRequestDto dto)
        {
            var success = await _service.UpdateAndApproveRequestAsync(id, dto);
            if (!success)
                return NotFound(new { status = "error", message = "فشل تحديث أو اعتماد الطلب" });

            return Ok(new { status = "success", message = "تم تحديث واعتماد طلب الخدمة بنجاح" });
        }

        // 7. استعراض قائمة مقدمي الخدمات المتاحين لإعادة الإسناد (شاشة مقدمو الخدمات)
        [HttpGet("providers-directory")]
        public async Task<IActionResult> GetProvidersDirectory([FromQuery] string? search, [FromQuery] int? subCategoryId)
        {
            var result = await _service.GetServiceProvidersDirectoryAsync(search, subCategoryId);
            return Ok(new { status = "success", data = result });
        }

        // 8. إعادة إسناد الطلب لمزود بديل وتوثيق السبب (شاشة إعادة الإسناد)
        [HttpPost("reassign/{requestId}")]
        public async Task<IActionResult> ReassignRequest(int requestId, [FromBody] ReassignServiceRequestDto dto)
        {
            var success = await _service.ReassignRequestAsync(requestId, dto);
            if (!success)
                return BadRequest(new { status = "error", message = "فشل إعادة إسناد الطلب، تأكد من صحة البيانات" });

            return Ok(new { status = "success", message = "تم إسناد الطلب للمزود الجديد وإشعار الطرفين بنجاح" });
        }

        [HttpGet("complaint/stats")]
        public async Task<IActionResult> GetComplaintStats()
        {
            var stats = await _service.GetStatsAsync();
            return Ok(new { status = "success", data = stats });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetComplaints([FromQuery] string status = "all", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var list = await _service.GetComplaintsAsync(status, page, pageSize);
            return Ok(new { status = "success", data = list });
        }

        [HttpPost("escalate/{id}")]
        public async Task<IActionResult> EscalateToHead(int id, [FromBody] ComplaintActionDto dto)
        {
            var success = await _service.EscalateComplaintAsync(id, dto);
            if (!success) return NotFound(new { status = "error", message = "الشكوى غير موجودة" });
            return Ok(new { status = "success", message = "تم تصعيد الشكوى لرئيس المشرفين بنجاح" });
        }

        [HttpPost("enforce-compensation/{id}")]
        public async Task<IActionResult> EnforceCompensation(int id, [FromBody] ComplaintActionDto dto)
        {
            var success = await _service.EnforceCompensationAsync(id, dto);
            if (!success) return NotFound(new { status = "error", message = "الشكوى غير موجودة" });
            return Ok(new { status = "success", message = "تم إقرار إلزام المزود بالتعويض بنجاح" });
        }

        [HttpGet("provider-professional-profile/{providerId}")]
        public async Task<IActionResult> GetProviderProfessionalProfile(int providerId)
        {
            var profile = await _service.GetProviderProfessionalProfileAsync(providerId);
            if (profile == null)
                return NotFound(new { status = "error", message = "مقدم الخدمة غير موجود" });

            return Ok(new { status = "success", data = profile });
        }

        // جلب تفاصيل المحفظة المالية المتقدمة للمشرف
        [HttpGet("advanced-wallet")]
        public async Task<IActionResult> GetAdvancedWallet()
        {
            var supervisorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var wallet = await _supervisorService.GetSupervisorWalletAsync(supervisorId);
            return Ok(new { status = "success", data = wallet });
        }

        // تقديم طلب سحب رصيد جديد مع تحديد طريقة التحويل
        [HttpPost("advanced-wallet/withdraw")]
        public async Task<IActionResult> RequestAdvancedWithdrawal([FromBody] WithdrawalRequestDtoAdmin dto)
        {
            var supervisorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _supervisorService.RequestWithdrawalAsync(supervisorId, dto);

            if (!success)
                return BadRequest(new { status = "error", message = "فشل تنفيذ طلب السحب، تأكد من الرصيد المتاح" });

            return Ok(new { status = "success", message = "تم تقديم طلب السحب المالي بنجاح وجاري التدقيق والاعتماد" });
        }

        // جلب بيانات الإعدادات والملف الشخصي للمشرف
        [HttpGet("account-settings")]
        public async Task<IActionResult> GetAccountSettings()
        {
            var supervisorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var settings = await _service.GetAccountSettingsAsync(supervisorId);
            return Ok(new { status = "success", data = settings });
        }
    }
}


