using Berwah.Models.Enum;
using Berwah.Models.Models;
using Berwah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdPromotionController : ControllerBase
    {
        private readonly AdPromotionService _promotions;

        public AdPromotionController(AdPromotionService promotions) => _promotions = promotions;

        public class PromoteRequest
        {
            public int AdvertisementId { get; set; }
            public PromotionPlanType Plan { get; set; }
            public AdPaymentMethod? PaymentMethod { get; set; }
            public string? PaymentReference { get; set; }
        }

        /// <summary>The plans, their prices and how long they last.</summary>
        [HttpGet("Plans")]
        public IActionResult Plans() => Ok(new
        {
            status = "success",
            data = new object[]
            {
                new { plan = "Free", price = 0, days = Berwah.Models.Models.AdPromotionRules.AdLifetimeDays },
                new { plan = "Featured", price = Berwah.Models.Models.AdPromotionRules.PriceOf(PromotionPlanType.Featured), days = Berwah.Models.Models.AdPromotionRules.DaysOf(PromotionPlanType.Featured) },
                new { plan = "VIP", price = Berwah.Models.Models.AdPromotionRules.PriceOf(PromotionPlanType.VIP), days = Berwah.Models.Models.AdPromotionRules.DaysOf(PromotionPlanType.VIP) }
            }
        });

        /// <summary>Asks for a Featured / VIP plan on one of my existing ads. It starts once an admin approves the payment.</summary>
        [Authorize]
        [HttpPost("Request")]
        public async Task<IActionResult> RequestPlan([FromBody] PromoteRequest request)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized(new { status = "fail", message = "غير مصرح لك بالوصول" });
            if (User.IsInRole("Merchant"))
                return StatusCode(403, new { status = "fail", message = "باقات الإعلانات متاحة للمستخدمين العاديين فقط" });

            var (error, purchase) = await _promotions.RequestForExistingAdAsync(
                request.AdvertisementId, userId, request.Plan, request.PaymentMethod, request.PaymentReference);
            if (error != null) return BadRequest(new { status = "fail", message = error });
            return Ok(new { status = "success", data = new { purchase!.Id, status = purchase.Status.ToString(), purchase.Amount } });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Pending")]
        public async Task<IActionResult> Pending() => Ok(new { status = "success", data = await _promotions.PendingAsync() });

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/Approve")]
        public async Task<IActionResult> Approve(int id) =>
            await _promotions.ApproveAsync(id)
                ? Ok(new { status = "success" })
                : NotFound(new { status = "fail", message = "الطلب غير موجود أو تمت مراجعته" });

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/Reject")]
        public async Task<IActionResult> Reject(int id) =>
            await _promotions.RejectAsync(id)
                ? Ok(new { status = "success" })
                : NotFound(new { status = "fail", message = "الطلب غير موجود أو تمت مراجعته" });
    }
}
