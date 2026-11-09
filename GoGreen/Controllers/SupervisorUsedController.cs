using Berwah.BL.Service.IService;
using Berwah.Models.DTO.SupervisorUsed;
using Berwah.Models.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupervisorUsedController : ControllerBase
    {
        private readonly ISupervisorUsedService _service;

        public SupervisorUsedController(ISupervisorUsedService service)
        {
            _service = service;
        }

        // 📊 1. إحصائيات لوحة التحكم (الشاشة الأولى - الكروت العلوية)
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _service.GetUsedAdsStatsAsync();
            return Ok(new { status = "success", data = stats });
        }

        // 📋 2. جلب قائمة الإعلانات المستعملة (مع الفلترة حسب الحالة: معلق، مراجع، الخ)
        [HttpGet]
        public async Task<IActionResult> GetUsedAds([FromQuery] AdStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var ads = await _service.GetUsedAdsByStatusAsync(status, page, pageSize);
            return Ok(new { status = "success", data = ads });
        }

        // 🔍 3. عرض تفاصيل وفحص إعلان معين (الشاشة الثانية - تفاصيل المنتج والصور والمقاييس)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsedAdDetails(int id)
        {
            var adDetails = await _service.GetUsedAdDetailsAsync(id);
            if (adDetails == null) return NotFound(new { message = "الإعلان غير موجود" });
            return Ok(new { status = "success", data = adDetails });
        }

        // ✅ 4. قبول الإعلان واعتماده للنشر (مع إمكانية تعديل السعر أو الوصف)
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveAd(int id, [FromBody] UsedAdActionDto dto)
        {
            var result = await _service.ApproveAndPublishAdAsync(id, dto);
            if (!result) return BadRequest(new { message = "فشل اعتماد الإعلان" });
            return Ok(new { status = "success", message = "تم اعتماد الإعلان ونشره بنجاح" });
        }

        // ✏️ 5. طلب تعديل من الناشر
        [HttpPost("{id}/request-edit")]
        public async Task<IActionResult> RequestEdit(int id, [FromBody] UsedAdActionDto dto)
        {
            var result = await _service.RequestEditFromPublisherAsync(id, dto.Reason);
            if (!result) return BadRequest(new { message = "فشل إرسال طلب التعديل" });
            return Ok(new { status = "success", message = "تم إرسال طلب التعديل للبائع بنجاح" });
        }

        // ⏸️ 6. تعليق الإعلان
        [HttpPost("{id}/suspend")]
        public async Task<IActionResult> SuspendAd(int id, [FromBody] UsedAdActionDto dto)
        {
            var result = await _service.SuspendAdAsync(id, dto.Reason);
            if (!result) return BadRequest(new { message = "فشل تعليق الإعلان" });
            return Ok(new { status = "success", message = "تم تعليق الإعلان بنجاح" });
        }

        // ❌ 7. رفض الإعلان ومخالفة المعلم (رفض مسبب مع عقوبة)
        [HttpPost("{id}/reject-with-penalty")]
        public async Task<IActionResult> RejectWithPenalty(int id, [FromBody] UsedAdActionDto dto)
        {
            var result = await _service.RejectAdWithPenaltyAsync(id, dto);
            if (!result) return BadRequest(new { message = "فشل رفض الإعلان" });
            return Ok(new { status = "success", message = "تم رفض الإعلان وتسجيل المخالفة على المعلم" });
        }

        // --- 📦 طلبات وصلني (Wosally Orders) ---
        [HttpGet("wosally-orders")]
        public async Task<IActionResult> GetWosallyOrders([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var orders = await _service.GetWosallyOrdersAsync(status, page, pageSize);
            return Ok(new { status = "success", data = orders });
        }

        [HttpGet("wosally-orders/{id}")]
        public async Task<IActionResult> GetWosallyOrderDetails(int id)
        {
            var details = await _service.GetWosallyOrderDetailsAsync(id);
            if (details == null) return NotFound(new { message = "الطلب غير موجود" });
            return Ok(new { status = "success", data = details });
        }

        [HttpPost("wosally-orders/{id}/approve-and-reveal")]
        public async Task<IActionResult> ApproveAndRevealData(int id, [FromBody] WosallyActionDto dto)
        {
            var result = await _service.ApproveAndRevealBuyerDataAsync(id, dto);
            if (!result) return BadRequest(new { message = "فشل تنفيذ العملية" });
            return Ok(new { status = "success", message = "تمت الموافقة وإظهار البيانات بنجاح" });
        }

        [HttpPost("wosally-orders/{id}/send-joint-alert")]
        public async Task<IActionResult> SendJointAlert(int id)
        {
            var result = await _service.SendJointAlertAsync(id);
            if (!result) return BadRequest(new { message = "فشل إرسال التنبيه" });
            return Ok(new { status = "success", message = "تم إرسال التنبيه المشترك للطرفين" });
        }

        // --- 🚨 إدارة الشكاوي والبلاغات (Complaints & Disputes) ---
        [HttpGet("complaints")]
        public async Task<IActionResult> GetComplaints([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var complaints = await _service.GetComplaintsAsync(status, page, pageSize);
            return Ok(new { status = "success", data = complaints });
        }

        [HttpPost("complaints/{id}/follow-up")]
        public async Task<IActionResult> FollowUpComplaint(int id, [FromBody] ComplaintActionForUsedDto dto)
        {
            var result = await _service.FollowUpComplaintAsync(id, dto);
            if (!result) return BadRequest(new { message = "فشل تحديث الشكوى" });
            return Ok(new { status = "success", message = "تم بدء المتابعة والتحقق الميداني بنجاح" });
        }

        [HttpPost("complaints/{id}/escalate")]
        public async Task<IActionResult> EscalateComplaint(int id, [FromBody] ComplaintActionForUsedDto dto)
        {
            var result = await _service.EscalateToBossAsync(id, dto);
            if (!result) return BadRequest(new { message = "فشل التصعيد" });
            return Ok(new { status = "success", message = "تم تصعيد الشكوى للرئيس بنجاح" });
        }

        [HttpGet("complaints/{id}/details")]
        public async Task<IActionResult> GetComplaintDetails(int id)
        {
            var details = await _service.GetDetailedComplaintAsync(id);
            if (details == null) return NotFound(new { status = "error", message = "الشكوى غير موجودة" });
            return Ok(new { status = "success", data = details });
        }

        [HttpPost("complaints/{id}/send-to-seller")]
        public async Task<IActionResult> SendToSeller(int id, [FromBody] ComplaintActionForUsedDto dto)
        {
            var result = await _service.SendComplaintToSellerAsync(id, dto);
            if (!result) return BadRequest(new { status = "error", message = "فشل إرسال الشكوى للبائع" });
            return Ok(new { status = "success", message = "تم إرسال الشكوى للبائع بنجاح" });
        }

        [HttpGet("wallet")]
        public async Task<IActionResult> GetWallet()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var wallet = await _service.GetWalletDataAsync(userId);
            return Ok(new { status = "success", data = wallet });
        }
    }
}
