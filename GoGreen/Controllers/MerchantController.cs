using Berwah.BL.Service.IService;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Identity;
using Berwah.DAL.App;
using Berwah.Models.DTO.Delivery;
using Berwah.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantController : ControllerBase
    {
        private readonly IMerchantService _merchantService;
        private readonly IProductService _productService;
        private readonly IImageService _imageService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notifications;

        public MerchantController(IMerchantService merchantService, IProductService productService,
            IImageService imageService, UserManager<User> userManager,
            ApplicationDbContext db, INotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
            _merchantService = merchantService;
            _productService = productService;
            _imageService = imageService;
            _userManager = userManager;
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
        public async Task<IActionResult> ToggleStoreStatus([FromQuery] bool isOpen)
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

                profile.TotalReviewsCount = await _db.StoreReviews.CountAsync(r => r.MerchantId == merchantId);
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

        // تحديث وقت التحضير المتوسط
        [Authorize(Roles = "Merchant")]
        [HttpPut("UpdatePreparationTime")]
        public async Task<IActionResult> UpdatePreparationTime([FromBody] UpdatePreparationTimeDto model)
        {
            try
            {
                var text = model.PreparationTime?.Trim();
                if (string.IsNullOrEmpty(text) || text.Length > 40)
                    return BadRequest(new { status = "error", message = "وقت التحضير غير صالح" });

                var merchantId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var user = await _userManager.FindByIdAsync(merchantId);
                if (user == null)
                    return NotFound(new { status = "error", message = "التاجر غير موجود" });

                user.DeliveryTimeSpan = text;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(new { status = "error", message = "تعذر حفظ وقت التحضير" });

                return Ok(new { status = "success", message = "تم تحديث وقت التحضير بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // تحديث موقع المحل (نقطة الاستلام للمندوب)
        [Authorize(Roles = "Merchant")]
        [HttpPut("UpdateLocation")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationDto model)
        {
            try
            {
                if (!Berwah.BL.Service.MerchantService.IsValidLocation(model.Latitude, model.Longitude))
                    return BadRequest(new { status = "error", message = "الموقع غير صالح" });

                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var success = await _merchantService.UpdateLocationAsync(merchantId, model.Latitude, model.Longitude);

                if (!success)
                    return NotFound(new { status = "error", message = "التاجر غير موجود" });

                return Ok(new { status = "success", message = "تم تحديث موقع المحل بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // تغيير صورة المحل
        [Authorize(Roles = "Merchant")]
        [HttpPut("UpdateProfileImage")]
        public async Task<IActionResult> UpdateProfileImage([FromForm] MerchantProfileImageForm model)
        {
            try
            {
                if (model.Image == null || model.Image.Length == 0)
                    return BadRequest(new { status = "error", message = "الصورة مطلوبة" });

                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (user == null) return NotFound(new { status = "error", message = "التاجر غير موجود" });

                var res = await _imageService.UploadImageAsync(model.Image);
                var url = res?.SecureUrl?.ToString();
                if (string.IsNullOrEmpty(url))
                    return StatusCode(500, new { status = "error", message = "فشل رفع الصورة" });

                user.ProfileImageUrl = url;
                await _userManager.UpdateAsync(user);
                return Ok(new { status = "success", message = "تم تحديث صورة المحل", data = new { profileImageUrl = url } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // تغيير أقسام المحل (مشويات، أسماك...)
        [Authorize(Roles = "Merchant")]
        [HttpPut("UpdateCategories")]
        public async Task<IActionResult> UpdateCategories([FromBody] MerchantCategoriesDto model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (user == null) return NotFound(new { status = "error", message = "التاجر غير موجود" });

                var ids = (model.SubCategoryIds ?? new List<int>()).Where(n => n > 0).Distinct().ToList();
                user.BusinessSubCategoryIds = ids.Count == 0 ? null : string.Join(",", ids);
                await _userManager.UpdateAsync(user);
                return Ok(new { status = "success", message = "تم تحديث أقسام المحل", data = new { subCategoryIds = ids } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        #region إدارة المنيو والأقسام للتاجر (Menu & Sections Endpoints)

        // 1. جلب أقسام المنيو الخاصة بالتاجر الحالي (سندويتشات، وجبات، إلخ)
        [Authorize(Roles = "Merchant")]
        [HttpGet("sections")]
        public async Task<IActionResult> GetMyMenuSections()
        {
            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var sections = await _productService.GetMerchantMenuSectionsAsync(merchantId);

                return Ok(new { status = "success", data = sections });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // 2. إنشاء قسم جديد في المنيو خاص بهذا التاجر
        [Authorize(Roles = "Merchant")]
        [HttpPost("Createsections")]
        public async Task<IActionResult> CreateMenuSection([FromBody] CreateMenuSectionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var success = await _productService.CreateMenuSectionAsync(merchantId, dto);

                if (!success)
                    return BadRequest(new { status = "error", message = "فشل إنشاء القسم الجديد" });

                return Ok(new { status = "success", message = "تم إنشاء القسم بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

  
        [Authorize(Roles = "Merchant")]
        [HttpGet("sections/{sectionId:int}/items")]
        public async Task<IActionResult> GetMenuItems(int sectionId)
        {
            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                // Only the items of this merchant's own section.
                var items = await _merchantService.GetSectionItemsAsync(merchantId, sectionId);
                return Ok(new { status = "success", data = items });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // The store page a customer sees: the store and its sections with their items.
        [Authorize]
        [HttpGet("{merchantId:int}/menu")]
        public async Task<IActionResult> GetStoreMenu(int merchantId)
        {
            try
            {
                var menu = await _merchantService.GetStoreMenuAsync(merchantId);
                if (menu == null)
                    return NotFound(new { status = "fail", message = "المحل غير موجود" });

                return Ok(new { status = "success", data = menu });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost("items")]
        public async Task<IActionResult> CreateMenuItem([FromForm] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var createdProduct = await _productService.CreateProductAsync(merchantId, dto);

                return Ok(new { status = "success", message = "تم إضافة الوجبة إلى المنيو بنجاح", data = createdProduct });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // تعديل منتج في المنيو
        [Authorize(Roles = "Merchant")]
        [HttpPut("items/{id:int}")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromForm] UpdateProductDto dto)
        {
            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var ok = await _productService.UpdateProductAsync(id, merchantId, dto);
                if (!ok) return NotFound(new { status = "error", message = "المنتج غير موجود" });
                return Ok(new { status = "success", message = "تم تحديث المنتج" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // حذف منتج من المنيو
        [Authorize(Roles = "Merchant")]
        [HttpDelete("items/{id:int}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            try
            {
                var merchantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var ok = await _productService.DeleteProductAsync(id, merchantId);
                if (!ok) return NotFound(new { status = "error", message = "المنتج غير موجود" });
                return Ok(new { status = "success", message = "تم حذف المنتج" });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return Conflict(new { status = "error", message = "لا يمكن حذف منتج مرتبط بطلبات سابقة. اجعل المخزون صفرًا لإخفائه." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        #endregion

        #region Pause, sections, wallet and reviews

        private int Me => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private const string WithdrawType = "MerchantWithdrawal";

        public class PauseDto
        {
            public int Minutes { get; set; }
        }

        // Closes the store for a while; it opens again by itself when the time is up.
        [Authorize(Roles = "Merchant")]
        [HttpPost("Pause")]
        public async Task<IActionResult> PauseStore([FromBody] PauseDto dto)
        {
            try
            {
                var minutes = Math.Clamp(dto.Minutes, 5, 240);
                var merchant = await _db.Users.FirstOrDefaultAsync(u => u.Id == Me);
                if (merchant == null) return NotFound(new { status = "error", message = "التاجر غير موجود" });
                merchant.PausedUntil = DateTime.UtcNow.AddMinutes(minutes);
                await _db.SaveChangesAsync();
                return Ok(new { status = "success", message = "تم إيقاف استقبال الطلبات مؤقتًا", data = new { pausedUntil = merchant.PausedUntil, minutes } });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost("Resume")]
        public async Task<IActionResult> ResumeStore()
        {
            try
            {
                var merchant = await _db.Users.FirstOrDefaultAsync(u => u.Id == Me);
                if (merchant == null) return NotFound(new { status = "error", message = "التاجر غير موجود" });
                merchant.PausedUntil = null;
                await _db.SaveChangesAsync();
                return Ok(new { status = "success", message = "عاد المتجر لاستقبال الطلبات" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        public class RenameSectionDto
        {
            public string? Title { get; set; }
        }

        [Authorize(Roles = "Merchant")]
        [HttpPut("sections/{sectionId:int}")]
        public async Task<IActionResult> RenameSection(int sectionId, [FromBody] RenameSectionDto dto)
        {
            try
            {
                var title = dto.Title?.Trim();
                if (string.IsNullOrEmpty(title) || title.Length > 60)
                    return BadRequest(new { status = "error", message = "اسم القسم غير صالح" });
                var section = await _db.menuSections.FirstOrDefaultAsync(s => s.Id == sectionId && s.MerchantId == Me);
                if (section == null) return NotFound(new { status = "error", message = "القسم غير موجود" });
                section.Name = title;
                await _db.SaveChangesAsync();
                return Ok(new { status = "success", message = "تم تعديل اسم القسم" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        // Only an empty section can go; its items have to be moved or deleted first.
        [Authorize(Roles = "Merchant")]
        [HttpDelete("sections/{sectionId:int}")]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            try
            {
                var section = await _db.menuSections.FirstOrDefaultAsync(s => s.Id == sectionId && s.MerchantId == Me);
                if (section == null) return NotFound(new { status = "error", message = "القسم غير موجود" });
                if (await _db.Products.AnyAsync(p => p.MenuSectionId == sectionId))
                    return Conflict(new { status = "error", message = "لا يمكن حذف قسم يحتوي على أصناف. انقل الأصناف أو احذفها أولًا." });
                _db.menuSections.Remove(section);
                await _db.SaveChangesAsync();
                return Ok(new { status = "success", message = "تم حذف القسم" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        private sealed record MerchantFigures(decimal Earned, decimal Withdrawn, decimal Available, decimal Percent);

        // The store's part of every delivered order (its items less the company's commission), less what it withdrew.
        private async Task<(MerchantFigures Figures, List<object> Ledger)> LoadWalletAsync(int merchantId)
        {
            var settings = await _db.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
            var percent = settings != null && settings.CompanyCommissionPercentage > 0 ? settings.CompanyCommissionPercentage : 10m;

            var orders = await _db.Orders.AsNoTracking()
                .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .Where(o => o.Status == OrderStatus.Delivered && o.OrderItems.Any(i => i.Product.UserId == merchantId))
                .OrderByDescending(o => o.DeliveredAt ?? o.CreatedAt)
                .ToListAsync();

            var rows = new List<(DateTime At, object Row)>();
            decimal earned = 0;
            foreach (var o in orders)
            {
                var gross = o.OrderItems
                    .Where(i => i.Product != null && i.Product.UserId == merchantId && i.MerchantStatus != OrderStatus.Cancelled)
                    .Sum(i => i.Price * i.Quantity);
                if (gross <= 0) continue;
                var net = Math.Round(gross * (100m - percent) / 100m, 2);
                earned += net;
                var at = o.DeliveredAt ?? o.CreatedAt;
                rows.Add((at, new { title = $"طلب #{o.Id}", description = $"طلب #{o.Id}", amount = net, type = "income", isIncome = true, createdAt = at }));
            }

            var withdrawals = await _db.Transactions.AsNoTracking()
                .Where(t => t.UserId == merchantId && t.Type == WithdrawType)
                .OrderByDescending(t => t.CreatedAt).ToListAsync();
            foreach (var w in withdrawals)
                rows.Add((w.CreatedAt, new { title = string.IsNullOrWhiteSpace(w.Description) ? "طلب سحب" : $"سحب {w.Description}", description = w.Description, amount = w.Amount, type = "withdrawal", isIncome = false, createdAt = w.CreatedAt }));

            var withdrawn = withdrawals.Sum(w => w.Amount);
            var ledger = rows.OrderByDescending(r => r.At).Take(60).Select(r => r.Row).ToList();
            return (new MerchantFigures(earned, withdrawn, Math.Max(0, earned - withdrawn), percent), ledger);
        }

        [Authorize(Roles = "Merchant")]
        [HttpGet("Wallet")]
        public async Task<IActionResult> GetWallet()
        {
            try
            {
                var (f, ledger) = await LoadWalletAsync(Me);
                var settings = await _db.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        available = f.Available,
                        totalEarned = f.Earned,
                        totalWithdrawn = f.Withdrawn,
                        commissionPercent = f.Percent,
                        minWithdrawal = settings?.MinWithdrawalAmount ?? 0m,
                        withdrawalFee = settings?.WithdrawalFeeFixed ?? 0m,
                        transactions = ledger
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost("Withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto model)
        {
            try
            {
                var me = Me;
                if (model.Amount <= 0)
                    return BadRequest(new { status = "error", message = "مبلغ السحب غير صالح" });

                var settings = await _db.SystemSettings.AsNoTracking().FirstOrDefaultAsync();
                decimal fee = settings?.WithdrawalFeeFixed ?? 0m;
                decimal minimum = settings?.MinWithdrawalAmount ?? 0m;
                if (model.Amount <= fee)
                    return BadRequest(new { status = "error", message = "مبلغ السحب يجب أن يكون أكبر من رسوم السحب المقرر" });
                if (model.Amount < minimum)
                    return BadRequest(new { status = "error", message = "مبلغ السحب أقل من الحد الأدنى" });

                var (f, _) = await LoadWalletAsync(me);
                if (model.Amount > f.Available)
                    return BadRequest(new { status = "error", message = "الرصيد المتاح لا يكفي لهذا المبلغ" });

                var account = (model.DestinationAccount ?? string.Empty).Trim();
                var tail = account.Length > 4 ? account[^4..] : account;
                _db.Transactions.Add(new Transaction
                {
                    UserId = me,
                    Amount = model.Amount,
                    Type = WithdrawType,
                    Description = $"{model.PaymentMethod} {tail}".Trim(),
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();

                try
                {
                    var admins = await _userManager.GetUsersInRoleAsync("Admin");
                    foreach (var admin in admins)
                        await _notifications.SendNotificationAsync(new SendNotification
                        {
                            UserId = admin.Id,
                            Title = "طلب سحب من تاجر 💸",
                            Message = $"طلب تاجر سحب {model.Amount} ج.م. راجعه."
                        });
                }
                catch { /* best effort */ }

                return Ok(new
                {
                    status = "success",
                    message = "تم تسجيل طلب السحب، وسيتم التحويل بعد مراجعة الإدارة.",
                    data = new { requestedAmount = model.Amount, companyFee = fee, netReceived = model.Amount - fee }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        public class StoreReviewDto
        {
            public int MerchantId { get; set; }
            public int Rating { get; set; }
            public string? Comment { get; set; }
        }

        // A customer rates a store of an order he received.
        [Authorize]
        [HttpPost("orders/{orderId:int}/review")]
        public async Task<IActionResult> ReviewStore(int orderId, [FromBody] StoreReviewDto dto)
        {
            try
            {
                var me = Me;
                if (dto.Rating < 1 || dto.Rating > 5)
                    return BadRequest(new { status = "error", message = "التقييم من 1 إلى 5" });

                var order = await _db.Orders.AsNoTracking()
                    .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == me);
                if (order == null) return NotFound(new { status = "error", message = "الطلب غير موجود" });
                if (order.Status != OrderStatus.Delivered)
                    return BadRequest(new { status = "error", message = "يمكن التقييم بعد استلام الطلب" });
                if (!order.OrderItems.Any(i => i.Product != null && i.Product.UserId == dto.MerchantId && i.MerchantStatus != OrderStatus.Cancelled))
                    return BadRequest(new { status = "error", message = "هذا المتجر ليس ضمن الطلب" });
                if (await _db.StoreReviews.AnyAsync(r => r.OrderId == orderId && r.MerchantId == dto.MerchantId))
                    return Conflict(new { status = "error", message = "قيّمت هذا المتجر على هذا الطلب من قبل" });

                var comment = dto.Comment?.Trim();
                _db.StoreReviews.Add(new StoreReview
                {
                    OrderId = orderId,
                    MerchantId = dto.MerchantId,
                    CustomerId = me,
                    Rating = dto.Rating,
                    Comment = string.IsNullOrEmpty(comment) ? null : (comment.Length > 400 ? comment[..400] : comment),
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();

                var merchant = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.MerchantId);
                if (merchant != null)
                {
                    merchant.Rating = Math.Round(await _db.StoreReviews.Where(r => r.MerchantId == dto.MerchantId).AverageAsync(r => (double)r.Rating), 2);
                    await _db.SaveChangesAsync();
                    try
                    {
                        await _notifications.SendNotificationAsync(new SendNotification
                        {
                            UserId = merchant.Id,
                            Title = "تقييم جديد ⭐",
                            Message = $"قيّمك عميل {dto.Rating} من 5 على الطلب #{orderId}."
                        });
                    }
                    catch { /* best effort */ }
                }
                return Ok(new { status = "success", message = "شكرًا لتقييمك" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        // The store's reviews, newest first.
        [Authorize(Roles = "Merchant")]
        [HttpGet("Reviews")]
        public async Task<IActionResult> GetReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var me = Me;
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 5, 50);
                var rows = await (from r in _db.StoreReviews.AsNoTracking()
                                  join u in _db.Users.AsNoTracking() on r.CustomerId equals u.Id
                                  where r.MerchantId == me
                                  orderby r.CreatedAt descending
                                  select new { r.Id, r.OrderId, r.Rating, r.Comment, r.Reply, r.CreatedAt, r.RepliedAt, u.FirstName, u.LastName })
                    .Skip((page - 1) * pageSize).Take(pageSize + 1).ToListAsync();

                var data = rows.Take(pageSize).Select(r => new
                {
                    id = r.Id,
                    orderId = r.OrderId,
                    customer = $"{r.FirstName} {r.LastName}".Trim(),
                    rating = r.Rating,
                    comment = r.Comment,
                    reply = r.Reply,
                    dateText = r.CreatedAt.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)
                }).ToList();
                var count = await _db.StoreReviews.CountAsync(r => r.MerchantId == me);
                var merchant = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == me);
                return Ok(new { status = "success", data, hasMore = rows.Count > pageSize, total = count, average = merchant?.Rating ?? 0 });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        public class ReplyDto
        {
            public string? Reply { get; set; }
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost("Reviews/{reviewId:int}/reply")]
        public async Task<IActionResult> ReplyToReview(int reviewId, [FromBody] ReplyDto dto)
        {
            try
            {
                var text = dto.Reply?.Trim();
                if (string.IsNullOrEmpty(text))
                    return BadRequest(new { status = "error", message = "اكتب الرد" });
                var review = await _db.StoreReviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.MerchantId == Me);
                if (review == null) return NotFound(new { status = "error", message = "التقييم غير موجود" });
                review.Reply = text.Length > 300 ? text[..300] : text;
                review.RepliedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                try
                {
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = review.CustomerId,
                        Title = "رد على تقييمك 💬",
                        Message = $"ردّ المتجر على تقييمك للطلب #{review.OrderId}."
                    });
                }
                catch { /* best effort */ }
                return Ok(new { status = "success", message = "تم إرسال الرد" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        #endregion
    }

    public class MerchantProfileImageForm
    {
        public IFormFile? Image { get; set; }
    }

    public class MerchantCategoriesDto
    {
        public List<int>? SubCategoryIds { get; set; }
    }
}

namespace Berwah.Controllers
{
    public class UpdatePreparationTimeDto
    {
        public string? PreparationTime { get; set; }
    }
}
