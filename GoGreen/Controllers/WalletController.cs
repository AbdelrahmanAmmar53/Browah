using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace Berwah.Controllers
{
    /// <summary>
    /// The customer's wallet: balance and history, charging it by a transfer the admin checks,
    /// and the admin's review of those transfers. Paying from it is part of Order/Checkout.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : ControllerBase
    {
        public const string KindTopUp = "TopUp";
        public const string KindRefund = "Refund";
        public const string KindPayment = "Payment";

        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IImageService _imageService;
        private readonly INotificationService _notifications;
        private readonly UserManager<User> _userManager;

        public WalletController(ApplicationDbContext context, IConfiguration config, IImageService imageService,
            INotificationService notifications, UserManager<User> userManager)
        {
            _context = context;
            _config = config;
            _imageService = imageService;
            _notifications = notifications;
            _userManager = userManager;
        }

        private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        private decimal MinTopUp => _config.GetValue<decimal?>("Wallet:MinTopUp") ?? 50m;
        private decimal MaxTopUp => _config.GetValue<decimal?>("Wallet:MaxTopUp") ?? 5000m;

        private record TransferMethod(string Key, string Name, string Number);

        /// <summary>
        /// The transfer methods from appsettings (the number to send to is optional and shown when set);
        /// when none are configured, the two usual ones are offered without a number.
        /// </summary>
        private List<TransferMethod> Methods()
        {
            var configured = _config.GetSection("Wallet:Methods").GetChildren()
                .Select(c => new TransferMethod(c["Key"] ?? "", c["Name"] ?? "", (c["Number"] ?? "").Trim()))
                .Where(m => m.Key.Length > 0 && m.Name.Length > 0)
                .ToList();
            return configured.Count > 0
                ? configured
                : new List<TransferMethod>
                {
                    new("vodafone_cash", "فودافون كاش", ""),
                    new("instapay", "إنستا باي", "")
                };
        }

        [HttpGet("Summary")]
        public async Task<IActionResult> Summary()
        {
            var userId = CurrentUserId;
            if (userId == 0) return Unauthorized();

            var entries = await _context.WalletEntries.AsNoTracking().Where(e => e.UserId == userId).ToListAsync();
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var pending = await _context.WalletTopUps.AsNoTracking()
                .Where(t => t.UserId == userId && t.Status == 0).SumAsync(t => (decimal?)t.Amount) ?? 0m;

            return Ok(new
            {
                status = "success",
                balance = entries.Sum(e => e.Amount),
                totalTopUp = entries.Where(e => e.Kind == KindTopUp).Sum(e => e.Amount),
                spentThisMonth = -entries.Where(e => e.Kind == KindPayment && e.CreatedAt >= monthStart).Sum(e => e.Amount),
                pendingTopUp = pending
            });
        }

        [HttpGet("Transactions")]
        public async Task<IActionResult> Transactions()
        {
            var userId = CurrentUserId;
            if (userId == 0) return Unauthorized();

            var entries = await _context.WalletEntries.AsNoTracking().Where(e => e.UserId == userId).ToListAsync();
            var waiting = await _context.WalletTopUps.AsNoTracking()
                .Where(t => t.UserId == userId && t.Status != 1).ToListAsync();

            var rows = entries.Select(e => new
            {
                id = $"e{e.Id}",
                kind = e.Kind,
                title = e.Description,
                amount = e.Amount,
                state = "done",
                orderId = e.OrderId,
                createdAt = e.CreatedAt
            }).Concat(waiting.Select(t => new
            {
                id = $"t{t.Id}",
                kind = KindTopUp,
                title = t.Status == 0 ? "طلب شحن قيد المراجعة" : "طلب شحن مرفوض" + (string.IsNullOrWhiteSpace(t.RejectReason) ? "" : $" ({t.RejectReason})"),
                amount = t.Amount,
                state = t.Status == 0 ? "pending" : "rejected",
                orderId = (int?)null,
                createdAt = t.CreatedAt
            })).OrderByDescending(r => r.createdAt).Take(200).ToList();

            return Ok(new { status = "success", data = rows });
        }

        [HttpGet("Settings")]
        public IActionResult Settings() => Ok(new
        {
            status = "success",
            minTopUp = MinTopUp,
            maxTopUp = MaxTopUp,
            methods = Methods().Select(m => new { key = m.Key, name = m.Name, number = m.Number })
        });

        public class TopUpForm
        {
            public decimal Amount { get; set; }
            public string Method { get; set; } = string.Empty;
            public string ReferenceNumber { get; set; } = string.Empty;
            public IFormFile? Proof { get; set; }
        }

        [HttpPost("TopUp")]
        public async Task<IActionResult> RequestTopUp([FromForm] TopUpForm model)
        {
            var userId = CurrentUserId;
            if (userId == 0) return Unauthorized();

            if (model.Amount < MinTopUp || model.Amount > MaxTopUp)
                return BadRequest(new { status = "error", message = $"المبلغ يجب أن يكون بين {MinTopUp:0.##} و {MaxTopUp:0.##} ج.م" });

            var method = Methods().FirstOrDefault(m => m.Key == model.Method);
            if (method == null)
                return BadRequest(new { status = "error", message = "طريقة التحويل غير متاحة" });

            var reference = (model.ReferenceNumber ?? "").Trim();
            if (reference.Length < 4 || reference.Length > 60)
                return BadRequest(new { status = "error", message = "أدخل رقم العملية كما يظهر في إيصال التحويل" });

            if (model.Proof == null || model.Proof.Length == 0)
                return BadRequest(new { status = "error", message = "صورة إيصال التحويل مطلوبة" });

            // One transfer can be claimed once.
            if (await _context.WalletTopUps.AnyAsync(t => t.ReferenceNumber == reference && t.Method == method.Key && t.Status != 2))
                return BadRequest(new { status = "error", message = "رقم العملية مستخدم من قبل" });

            if (await _context.WalletTopUps.CountAsync(t => t.UserId == userId && t.Status == 0) >= 3)
                return BadRequest(new { status = "error", message = "لديك 3 طلبات شحن قيد المراجعة، انتظر اعتمادها أولاً" });

            var upload = await _imageService.UploadImageAsync(model.Proof);
            var url = upload?.SecureUrl?.ToString();
            if (string.IsNullOrEmpty(url))
                return StatusCode(500, new { status = "error", message = "تعذر رفع صورة الإيصال، حاول مرة أخرى" });

            var topUp = new WalletTopUp
            {
                UserId = userId,
                Amount = model.Amount,
                Method = method.Key,
                ReferenceNumber = reference,
                ProofImageUrl = url
            };
            _context.WalletTopUps.Add(topUp);
            await _context.SaveChangesAsync();

            try
            {
                foreach (var admin in await _userManager.GetUsersInRoleAsync("Admin"))
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = admin.Id,
                        Title = "طلب شحن محفظة جديد",
                        Message = $"طلب شحن بمبلغ {model.Amount:0.##} ج.م عبر {method.Name} يحتاج مراجعتك."
                    });
            }
            catch { }

            return Ok(new { status = "success", message = "تم إرسال طلب الشحن، سيُضاف الرصيد بعد مراجعة التحويل", id = topUp.Id });
        }

        // ---------- Admin ----------

        [Authorize(Roles = "Admin")]
        [HttpGet("Admin/TopUps")]
        public async Task<IActionResult> AdminTopUps([FromQuery] int? status)
        {
            var query = _context.WalletTopUps.AsNoTracking().Include(t => t.User).AsQueryable();
            if (status != null) query = query.Where(t => t.Status == status);
            var rows = await query.OrderByDescending(t => t.CreatedAt).Take(200).ToListAsync();
            var names = Methods().ToDictionary(m => m.Key, m => m.Name);

            return Ok(new
            {
                status = "success",
                data = rows.Select(t => new
                {
                    id = t.Id,
                    userId = t.UserId,
                    userName = $"{t.User.FirstName} {t.User.LastName}".Trim(),
                    phone = t.User.PhoneNumber,
                    amount = t.Amount,
                    method = names.TryGetValue(t.Method, out var n) ? n : t.Method,
                    referenceNumber = t.ReferenceNumber,
                    proofImageUrl = t.ProofImageUrl,
                    status = t.Status,
                    rejectReason = t.RejectReason,
                    createdAt = t.CreatedAt,
                    reviewedAt = t.ReviewedAt
                })
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Admin/TopUps/{id}/Approve")]
        public async Task<IActionResult> Approve(int id)
        {
            // Serializable: two admins pressing approve together cannot credit the same transfer twice.
            await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var topUp = await _context.WalletTopUps.FirstOrDefaultAsync(t => t.Id == id);
            if (topUp == null) return NotFound(new { status = "error", message = "الطلب غير موجود" });
            if (topUp.Status != 0) return BadRequest(new { status = "error", message = "تمت مراجعة هذا الطلب من قبل" });

            topUp.Status = 1;
            topUp.ReviewedAt = DateTime.UtcNow;
            topUp.ReviewedById = CurrentUserId;
            _context.WalletEntries.Add(new WalletEntry
            {
                UserId = topUp.UserId,
                Amount = topUp.Amount,
                Kind = KindTopUp,
                Description = "شحن المحفظة"
            });
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            try
            {
                await _notifications.SendNotificationAsync(new SendNotification
                {
                    UserId = topUp.UserId,
                    Title = "تم شحن محفظتك",
                    Message = $"تمت إضافة {topUp.Amount:0.##} ج.م إلى رصيد محفظتك.",
                    Type = "wallet"
                });
            }
            catch { }

            return Ok(new { status = "success", message = "تم اعتماد الشحن وإضافة الرصيد" });
        }

        public class RejectTopUpDto { public string? Reason { get; set; } }

        [Authorize(Roles = "Admin")]
        [HttpPost("Admin/TopUps/{id}/Reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectTopUpDto? model)
        {
            var topUp = await _context.WalletTopUps.FirstOrDefaultAsync(t => t.Id == id);
            if (topUp == null) return NotFound(new { status = "error", message = "الطلب غير موجود" });
            if (topUp.Status != 0) return BadRequest(new { status = "error", message = "تمت مراجعة هذا الطلب من قبل" });

            topUp.Status = 2;
            topUp.RejectReason = string.IsNullOrWhiteSpace(model?.Reason) ? null : model!.Reason!.Trim();
            topUp.ReviewedAt = DateTime.UtcNow;
            topUp.ReviewedById = CurrentUserId;
            await _context.SaveChangesAsync();

            try
            {
                await _notifications.SendNotificationAsync(new SendNotification
                {
                    UserId = topUp.UserId,
                    Title = "تعذر اعتماد طلب الشحن",
                    Message = topUp.RejectReason == null
                        ? $"لم يتم اعتماد طلب شحن {topUp.Amount:0.##} ج.م. تواصل مع الدعم لو تم التحويل."
                        : $"لم يتم اعتماد طلب شحن {topUp.Amount:0.##} ج.م: {topUp.RejectReason}",
                    Type = "wallet"
                });
            }
            catch { }

            return Ok(new { status = "success", message = "تم رفض الطلب" });
        }
    }
}
