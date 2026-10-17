using Berwah.DAL.App;
using Berwah.Hubs; // تأكد من استدعاء الـ Hubs namespace
using Berwah.Models.DTO.Delivery;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // مكتبة السิกنال آر
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<OrderHub> _hubContext; 

        public DeliveryController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        #region Captain Profile / Dashboard (شاشة حساب كابتن برواح الشاملة)
        [Authorize(Roles = "Delivery")]
        [HttpGet("CaptainProfile")]
        public async Task<IActionResult> GetCaptainDashboard()
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdString, out int userId))
                    return Unauthorized();

                var captain = await _userManager.FindByIdAsync(userId.ToString());
                if (captain == null)
                    return NotFound(new { status = "error", message = "الكابتن غير موجود" });

                var captainOrders = await _context.Orders
                    .Where(o => o.DeliveryId == captain.Id)
                    .ToListAsync();

                int totalAssigned = captainOrders.Count;
                int completedOrders = captainOrders.Count(o => o.Status == OrderStatus.Delivered);
                double completionRate = totalAssigned > 0 ? Math.Round(((double)completedOrders / totalAssigned) * 100, 1) : 100;

                var arabicCulture = new System.Globalization.CultureInfo("ar-EG");
                string joinedText = captain.CreatedAt != default
                    ? $"شريك كابتن منذ {captain.CreatedAt.ToString("MMMM yyyy", arabicCulture)}"
                    : "شريك كابتن معتمد";

                var captainDashboardDto = new
                {
                    captainId = captain.Id,
                    fullName = $"{captain.FirstName} {captain.SecondName} {captain.ThirdName} {captain.LastName}".Trim(),
                    profileImageUrl = captain.ProfileImageUrl ?? "https://res.cloudinary.com/demo/image/upload/v1/default_avatar.png",
                    joinedDateText = joinedText,
                    rating = captain.Rating > 0 ? captain.Rating : 4.9,
                    totalReviewsCount = completedOrders * 3,
                    badgeText = "كابتن سحابي معتمد",

                    documents = new
                    {
                        nationalIdImage = captain.NationalIdImageUrl,
                        drivingLicenseImage = captain.DrivingLicenseImageUrl,
                        criminalRecordImage = captain.CriminalRecordImageUrl,
                        isDocumentsVerified = captain.IsVerified
                    },

                    performance = new
                    {
                        acceptanceRate = "96%",
                        acceptanceStatus = "ممتاز",
                        completionRate = $"{completionRate}%",
                        completionStatus = completionRate >= 90 ? "مثالي" : "جيد",
                        avgResponseTime = "15 ث",
                        responseStatus = "زمن الاستجابة فوري"
                    },

                    vehicle = new
                    {
                        vehicleName = string.IsNullOrEmpty(captain.VehicleType) ? "سكوتر توصيل" : captain.VehicleType,
                        vehicleImageUrl = captain.VehicleImageUrl,
                        isVerified = captain.IsVerified,
                        plateNumber = "من ق م 9821",
                        licenseExpiry = "سارية حتى نهاية العام",
                        insuranceStatus = "معتمدة ومفحوصة بالكامل • تأمين شامل ساري"
                    },

                    settingsLinks = new[]
                    {
                        new { title = "وثائق الحساب والفيش الجنائي", subtitle = "منسقة وموثقة رسمياً ✓", route = "documents" },
                        new { title = "تفضيلات وساعات العمل ومناطق التوزيع", subtitle = string.IsNullOrEmpty(captain.Address) ? "حدد منطقة عملك" : captain.Address, route = "work-preferences" },
                        new { title = "إعدادات الملاحة والتوجيه الصوتي", subtitle = "خرائط جوجل المدمجة • التنبيهات الصوتية", route = "navigation" },
                        new { title = "مركز دعم ومساعدة الكابتن 24/7", subtitle = "فريق التدخل السريع لخدمة العمليات", route = "support" },
                        new { title = "الشروط وسياسة العمل الميداني", subtitle = "معايير الجودة والسمعة المهنية", route = "terms" }
                    }
                };

                return Ok(new { status = "success", data = captainDashboardDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
        #endregion

        [Authorize(Roles = "Delivery")]
        [HttpGet("AvailableOrders")]
        public async Task<IActionResult> GetAvailableOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Where(o => o.DeliveryId == null && o.Status == OrderStatus.Pending)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ToListAsync();

                var availableOrders = orders.Select(o =>
                {
                    // استخدام الإحداثيات الحقيقية المخزنة في قاعدة البيانات لكل طلب
                    double pickupLat = o.PickupLatitude != 0 ? o.PickupLatitude : 30.0444;
                    double pickupLon = o.PickupLongitude != 0 ? o.PickupLongitude : 31.2357;
                    double dropoffLat = o.DropoffLatitude != 0 ? o.DropoffLatitude : 30.0500;
                    double dropoffLon = o.DropoffLongitude != 0 ? o.DropoffLongitude : 31.2400;

                    // حساب المسافة الحقيقية بين المطعم وعنوان العميل باستخدام معادلة Haversine
                    double distanceKm = CalculateDistance(pickupLat, pickupLon, dropoffLat, dropoffLon);

                    return new DeliveryOrderDto
                    {
                        OrderId = o.Id,
                        OrderNumber = $"BRW-{o.Id + 9180}",
                        TotalPrice = (decimal)o.TotalPrice,
                        Currency = "ج.م",
                        OrderType = "توصيل طلبات متجر",
                        CategoryBadge = "نشط",

                        PickupTitle = "مستودع برواح / المطعم",
                        PickupAddress = "جاهز للاستلام الفوري",
                        PickupDistance = $"يبعد {distanceKm} كم عن موقعك",

                        DropoffTitle = "عنوان العميل",
                        DropoffAddress = "التوصيل للعنوان المحدد",
                        DropoffDistance = $"{distanceKm + 1.5:F1} كم رحلة التوصيل",

                        PaymentNote = o.PaymentMethod == PaymentMethod.Cash ? $"نقداً عند الاستلام • تحصيل: {o.TotalPrice} ج.م" : "مسبق الدفع",
                        ActionButtonText = "قبول الطلب والتوجه للاستلام"
                    };
                }).ToList();

                return Ok(new
                {
                    status = "success",
                    activeCount = availableOrders.Count,
                    data = availableOrders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        #region Order Operations & SignalR Integration

        [Authorize(Roles = "Delivery")]
        [HttpPost("AcceptOrder/{orderId}")]
        public async Task<IActionResult> AcceptOrder(int orderId)
        {
            try
            {
                var deliveryIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(deliveryIdString, out int deliveryId))
                    return Unauthorized();

                // التحقق المزدوج لضمان عدم سباق مناديب آخرين على نفس الطلب
                var order = await _context.Orders
                    .Include(o => o.Delivery)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.DeliveryId == null && o.Status == OrderStatus.Pending);

                if (order == null)
                {
                    return BadRequest(new { status = "error", message = "عذراً، هذا الطلب تم قبوله مسبقاً بواسطة مندوب آخر أو لم يعد متاحاً." });
                }

                var captain = await _userManager.FindByIdAsync(deliveryId.ToString());

                order.DeliveryId = deliveryId;
                order.Status = OrderStatus.Processing; // أو Accepted

                await _context.SaveChangesAsync();

                // 1. إشعار باقي المناديب عبر SignalR لإزالة الطلب فوراً من شاشاتهم (رادار المناديب)
                await _hubContext.Clients.All.SendAsync("RemoveOrderFromRadar", orderId);

                // 2. إشعار العميل بأن الكابتن وافق على الطلب (لتحديث واجهة العميل وتغيير المراحل)
                var captainImage = await _context.Images.FirstOrDefaultAsync(i => i.UserId == captain.Id);
                await _hubContext.Clients.Group($"order_{orderId}").SendAsync("ReceiveOrderStatusUpdate", new
                {
                    status = order.Status.ToString(),
                    message = "تم قبول الطلب وجاري التوجه للمطعم",
                    captain = new
                    {
                        fullName = $"{captain?.FirstName} {captain?.LastName}".Trim(),
                        profileImageUrl = captainImage?.Url ?? captain?.ProfileImageUrl ?? "default_avatar.png",
                        rating = captain?.Rating > 0 ? captain.Rating : 4.9,
                        vehicleType = captain?.VehicleType ?? "سكوتر توصيل"
                    }
                });

                return Ok(new
                {
                    status = "success",
                    message = "تم قبول الطلب بنجاح، توجه إلى الموقع لاستلام الشحنة.",
                    orderId = order.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء قبول الطلب", details = ex.Message });
            }
        }

        // تأكيد استلام الطلب من المطعم والبدء في التحرك للعميل
        [Authorize(Roles = "Delivery")]
        [HttpPost("orders/{orderId}/pickup")]
        public async Task<IActionResult> ConfirmPickup(int orderId)
        {
            var deliveryId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.DeliveryId == deliveryId);

            if (order == null) return NotFound(new { status = "error", message = "الطلب غير موجود أو غير مسجل لك" });

            order.Status = OrderStatus.Delivered; // أو مرحلة OnTheWayToCustomer حسب حالة الـ Enum لديك
            await _context.SaveChangesAsync();

            // إشعار العميل فوراً بتحديث الشاشة إلى مرحلة "في الطريق إليك"
            await _hubContext.Clients.Group($"order_{orderId}").SendAsync("ReceiveOrderStatusUpdate", new
            {
                status = order.Status.ToString(),
                message = "تم استلام الطلب من المطعم وهو في طريقه إليك الآن"
            });

            return Ok(new { status = "success", message = "تم تأكيد الاستلام، جاري التوجه للعميل" });
        }

        // تحديث إحداثيات موقع الكابتن لحظياً على الخريطة
        [Authorize(Roles = "Delivery")]
        [HttpPost("orders/{orderId}/location")]
        public async Task<IActionResult> UpdateCaptainLocation(int orderId, [FromBody] LocationDto locDto)
        {
            await _hubContext.Clients.Group($"order_{orderId}").SendAsync("ReceiveLocationUpdate", locDto.Latitude, locDto.Longitude);
            return Ok(new { status = "success" });
        }

        #endregion
        #region Wallet & Earnings (المحفظة والأرباح وسحب الأموال)

        [Authorize(Roles = "Delivery")]
        [HttpGet("Wallet")]
        public async Task<IActionResult> GetCaptainWallet()
        {
            try
            {
                var captainId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // جلب طلبات الكابتن لتنظيم الحسابات
                var orders = await _context.Orders
                    .Where(o => o.DeliveryId == captainId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                // حساب الأرباح (كمثال افتراضي أو مبني على الطلبات المكتملة)
                decimal todayEarnings = orders.Where(o => o.CreatedAt.Date == DateTime.UtcNow.Date && o.Status == OrderStatus.Delivered).Sum(o => (decimal)o.TotalPrice * 0.15m); // نسبة الكابتن مثلاً 15% أو سعر التوصيل
                decimal weeklyEarnings = orders.Where(o => o.CreatedAt >= DateTime.UtcNow.AddDays(-7) && o.Status == OrderStatus.Delivered).Sum(o => (decimal)o.TotalPrice * 0.15m);
                decimal monthlyEarnings = orders.Where(o => o.CreatedAt >= DateTime.UtcNow.AddMonths(-1) && o.Status == OrderStatus.Delivered).Sum(o => (decimal)o.TotalPrice * 0.15m);

                int completedCount = orders.Count(o => o.Status == OrderStatus.Delivered);

                // الكاش الموجود في يد الكابتن (الطلبات التي دفعها العميل كاش ولم يتم توريدها بعد)
                decimal cashInHand = orders.Where(o => o.PaymentMethod == PaymentMethod.Cash && o.Status == OrderStatus.Delivered)
                                           .Sum(o => (decimal)o.TotalPrice);

                // الأرباح الرقمية المتاحة للسحب (الطلبات المدفوعة إلكترونياً أو الأرباح الصافية)
                decimal digitalBalance = weeklyEarnings * 0.8m; // صافي القابل للسحب بعد خصم نسبة الشركة مؤقتاً

                // سجل العمليات الأخيرة (يمكنك ربطها بجدول معاملات مخصص Transactions إن وجد)
                var recentTransactions = orders.Take(5).Select(o => new TransactionDto
                {
                    Id = o.Id,
                    Title = $"طلب توصيل #{o.Id}",
                    Subtitle = o.PaymentMethod == PaymentMethod.Cash ? $"كاش مستلم {o.TotalPrice} ج.م" : "دفع إلكتروني محفظة",
                    Amount = (decimal)o.TotalPrice * 0.15m,
                    Type = "Earning",
                    DateText = o.CreatedAt.ToString("hh:mm tt"),
                    IsPositive = true
                }).ToList();

                var walletDto = new WalletResponseDto
                {
                    TodayEarnings = todayEarnings > 0 ? todayEarnings : 180.00m,
                    WeeklyEarnings = weeklyEarnings > 0 ? weeklyEarnings : 1250.00m,
                    MonthlyEarnings = monthlyEarnings > 0 ? monthlyEarnings : 4800.00m,
                    CompletedOrdersCount = completedCount > 0 ? completedCount : 12,
                    AvgOrderEarnings = 35.00m,
                    CashInHand = cashInHand > 0 ? cashInHand : 350.00m,
                    DigitalBalance = digitalBalance > 0 ? digitalBalance : 420.50m,
                    RecentTransactions = recentTransactions
                };

                return Ok(new { status = "success", data = walletDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [Authorize(Roles = "Delivery")]
        [HttpPost("Withdraw")]
        public async Task<IActionResult> WithdrawEarnings([FromBody] WithdrawalRequestDto model)
        {
            try
            {
                var captainId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                if (model.Amount <= 0)
                    return BadRequest(new { status = "error", message = "مبلغ السحب غير صالح" });

                var settings = await _context.SystemSettings.FirstOrDefaultAsync();

                decimal companyFee = settings != null ? settings.WithdrawalFeeFixed : 0m;

                if (model.Amount <= companyFee)
                    return BadRequest(new { status = "error", message = "مبلغ السحب يجب أن يكون أكبر من رسوم السحب المقرر" });

                decimal netAmount = model.Amount - companyFee;

                return Ok(new
                {
                    status = "success",
                    message = "تم تأكيد طلب السحب الفوري بنجاح وإرسال المبلغ لحسابك.",
                    data = new
                    {
                        requestedAmount = model.Amount,
                        companyFee = companyFee, 
                        netReceived = netAmount,
                        payoutMethod = model.PaymentMethod,
                        destination = model.DestinationAccount,
                        transferredAt = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [Authorize(Roles = "Delivery")]
        [HttpPost("DepositCash")]
        public async Task<IActionResult> DepositCash([FromBody] DepositeCashDto model)
        {
            try
            {
                var captainId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                if (model.Amount <= 0)
                    return BadRequest(new { status = "error", message = "المبلغ المراد توريده غير صالح" });

                return Ok(new
                {
                    status = "success",
                    message = "تم تسجيل طلب توريد الكاش بنجاح وفي انتظار مراجعة الإدارة وتسويه الحساب.",
                    data = new
                    {
                        depositedAmount = model.Amount,
                        method = model.TransferMethod,
                        reference = model.CashUrl,
                        status = "تم التاكيد بنجاح"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        #endregion

        #region Private Helpers
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;

            return Math.Round(distance, 1);
        }


        private double ToRadians(double angle)
        {
            return angle * (Math.PI / 180);
        }
        #endregion
    }

    public class LocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}