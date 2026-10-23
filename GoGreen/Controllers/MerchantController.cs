using Berwah.BL.Service.IService;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantController : ControllerBase
    {
        private readonly IMerchantService _merchantService;
        private readonly IProductService _productService;
        public MerchantController(IMerchantService merchantService, IProductService productService)
        {
            _merchantService = merchantService;
            _productService = productService;
        }

        [Authorize(Roles = "Merchant")]
        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] string? filter = "all")
        {
            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var dashboardDto = await _merchantService.GetDashboardDataAsync(merchantId, filter);

                return Ok(new { status = "success", data = dashboardDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost("Orders/{orderId}/UpdateStatus")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateMerchantOrderStatusDto model)
        {
            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var success = await _merchantService.UpdateOrderStatusAsync(orderId, merchantId, model);

                if (!success)
                    return NotFound(new { status = "error", message = "الطلب غير موجود أو غير تابع لمتجرك" });

                return Ok(new { status = "success", message = "تم تحديث حالة الطلب بنجاح وإرسال الإشعار" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost("ToggleStoreStatus")]
        public async Task<IActionResult> ToggleStoreStatus([FromBody] bool isOpen)
        {
            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var success = await _merchantService.ToggleStoreStatusAsync(merchantId, isOpen);

                if (!success)
                    return NotFound(new { status = "error", message = "التاجر غير موجود" });

                return Ok(new { status = "success", message = isOpen ? "تم فتح المتجر لاستقبال الطلبات بنجاح" : "تم إيقاف استقبال الطلبات مؤقتاً" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        #region Rejection Flow (شاشة تأكيد الاعتذار والغرامة)

        [Authorize(Roles = "Merchant")]
        [HttpGet("Orders/{orderId}/RejectDetails")]
        public async Task<IActionResult> GetRejectOrderDetails(int orderId)
        {
            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var details = await _merchantService.GetRejectOrderDetailsAsync(orderId, merchantId);

                if (details == null)
                    return NotFound(new { status = "error", message = "الطلب غير موجود أو غير تابع لمتجرك" });

                return Ok(new { status = "success", data = details });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost("Orders/{orderId}/ConfirmRejection")]
        public async Task<IActionResult> ConfirmOrderRejection(int orderId, [FromBody] ConfirmRejectOrderDto model)
        {
            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var success = await _merchantService.ConfirmOrderRejectionAsync(orderId, merchantId, model);

                if (!success)
                    return NotFound(new { status = "error", message = "فشل إلغاء الطلب، تأكد من صحة رقم الطلب" });

                return Ok(new { status = "success", message = "تم إلغاء الطلب وتطبيق الخصم المالي الموضح بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        #endregion
        [Authorize(Roles = "Merchant")]
        [HttpGet("Profile")]
        public async Task<IActionResult> GetMerchantProfile()
        {
            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var profile = await _merchantService.GetMerchantProfileAsync(merchantId);

                if (profile == null)
                    return NotFound(new { status = "error", message = "حساب التاجر غير موجود" });

                return Ok(new { status = "success", data = profile });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // تحديث مواعيد وساعات العمل
        [Authorize(Roles = "Merchant")]
        [HttpPut("UpdateWorkHours")]
        public async Task<IActionResult> UpdateWorkHours([FromBody] UpdateWorkHourDto model)
        {
            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var success = await _merchantService.UpdateWorkHoursAsync(merchantId, model.WorkHours);

                if (!success)
                    return NotFound(new { status = "error", message = "التاجر غير موجود" });

                return Ok(new { status = "success", message = "تم تحديث مواعيد العمل بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
        
        [HttpGet("sections/{mainCategoryId:int}")]
        public async Task<IActionResult> GetMenuSections(int mainCategoryId)
        {
            var sections = await _productService.GetMenuSectionsByMainCategoryIdAsync(mainCategoryId);
            return Ok(new { status = "success", data = sections });
        }

        // GET api/menu/sections/{subCategoryId}/items
        // returns menu items (products) for a subcategory
        [HttpGet("sections/{subCategoryId:int}/items")]
        public async Task<IActionResult> GetMenuItems(int subCategoryId)
        {
            var items = await _productService.GetProductsBySubCategoryAsync(subCategoryId);
            return Ok(new { status = "success", data = items });
        }

        // POST api/menu/items
        // create a new menu item (merchant only)
        [Authorize(Roles = "Merchant")]
        [HttpPost("items")]
        public async Task<IActionResult> CreateMenuItem([FromForm] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var userId)) return Unauthorized(new { status = "fail", message = "Invalid user" });

            await _productService.CreateProductAsync(userId, dto);

            return Ok(new { status = "success", message = "Menu item created" });
        }

        [HttpGet("by-business-type/{businessType}")]
        public async Task<IActionResult> GetMerchantsByBusinessType(string businessType)
        {
            try
            {
                var merchants = await _merchantService.GetMerchantsByBusinessTypeAsync(businessType);
                return Ok(new { status = "success", data = merchants });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}   