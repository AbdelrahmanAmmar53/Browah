using Berwah.BL.Service;
using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Hubs; // تأكد من استدعاء الـ Hubs namespace
using Berwah.Models.DTO;
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
        private readonly INotificationService _notifications;
        private readonly IImageService _imageService;

        /// <summary>The last position each courier sent, by order, so the customer's tracking map can show it.</summary>
        public static readonly System.Collections.Concurrent.ConcurrentDictionary<int, (double Lat, double Lng)> CaptainPositions = new();

        public DeliveryController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IHubContext<OrderHub> hubContext,
            INotificationService notifications,
            IImageService imageService)
        {
            _imageService = imageService;
            _notifications = notifications;
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

                var responseSeconds = captainOrders
                    .Where(o => o.CourierAcceptedAt != null && o.FirstOfferedAt != null)
                    .Select(o => (o.CourierAcceptedAt!.Value - o.FirstOfferedAt!.Value).TotalSeconds)
                    .Where(s => s >= 0).ToList();
                double avgSeconds = responseSeconds.Count > 0 ? responseSeconds.Average() : 0;
                var ratedList = captainOrders.Where(o => o.CourierRating != null).ToList();
                int released = captain.ReleasedOrdersCount;
                double acceptedShare = totalAssigned + released > 0 ? totalAssigned * 100.0 / (totalAssigned + released) : 0;
                bool hasAcceptanceData = totalAssigned + released > 0;

                var captainDashboardDto = new
                {
                    captainId = captain.Id,
                    fullName = $"{captain.FirstName} {captain.SecondName} {captain.ThirdName} {captain.LastName}".Trim(),
                    profileImageUrl = captain.ProfileImageUrl,
                    joinedDateText = joinedText,
                    rating = ratedList.Count > 0 ? Math.Round(ratedList.Average(o => o.CourierRating!.Value), 1) : 0.0,
                    totalReviewsCount = ratedList.Count,
                    badgeText = captain.IsVerified ? "كابتن معتمد" : "كابتن قيد المراجعة",

                    documents = new
                    {
                        nationalIdImage = captain.NationalIdImageUrl,
                        drivingLicenseImage = captain.DrivingLicenseImageUrl,
                        criminalRecordImage = captain.CriminalRecordImageUrl,
                        isDocumentsVerified = captain.IsVerified
                    },

                    performance = new
                    {
                        acceptanceRate = hasAcceptanceData ? $"{Math.Round(acceptedShare)}%" : "—",
                        acceptanceStatus = !hasAcceptanceData ? "لا توجد بيانات كافية" : (acceptedShare >= 90 ? "ممتاز" : "جيد"),
                        completionRate = totalAssigned > 0 ? $"{completionRate}%" : "—",
                        completionStatus = totalAssigned == 0 ? "لا توجد بيانات كافية" : (completionRate >= 90 ? "مثالي" : "جيد"),
                        avgResponseTime = responseSeconds.Count == 0 ? "—"
                            : avgSeconds < 60 ? $"{Math.Round(avgSeconds)} ث" : $"{Math.Round(avgSeconds / 60)} دقيقة",
                        responseStatus = responseSeconds.Count == 0 ? "لا توجد بيانات كافية"
                            : avgSeconds <= 120 ? "زمن الاستجابة فوري" : avgSeconds <= 600 ? "جيد" : "بطيء"
                    },

                    vehicle = new
                    {
                        vehicleName = string.IsNullOrEmpty(captain.VehicleType) ? "سكوتر توصيل" : captain.VehicleType,
                        vehicleImageUrl = captain.VehicleImageUrl,
                        isVerified = captain.IsVerified,
                        plateNumber = captain.VehiclePlateNumber ?? "",
                        licenseExpiry = captain.DrivingLicenseExpiry.HasValue ? $"سارية حتى {captain.DrivingLicenseExpiry.Value:yyyy-MM-dd}" : "",
                        insuranceStatus = captain.IsVerified ? "الوثائق موثقة من الإدارة" : "الوثائق قيد المراجعة"
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
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }
        #endregion

        // One order as the radar and the trip show it. The courier's own position (when the app sent it)
        // gives the distance to the store; without positions no distance is shown rather than a made-up one.
        private DeliveryOrderDto BuildOrderDto(Order o, double? courierLat, double? courierLng)
        {
            // The stores the order is picked up from (one per merchant in the cart).
            var stops = o.OrderItems
                .Where(oi => oi.Product?.User != null && oi.MerchantStatus != OrderStatus.Cancelled)
                .GroupBy(oi => oi.Product!.User!.Id)
                .Select(g =>
                {
                    var u = g.First().Product!.User!;
                    return new DeliveryStopDto
                    {
                        MerchantId = u.Id,
                        StoreName = string.IsNullOrWhiteSpace(u.BusinessName) ? $"{u.FirstName} {u.LastName}".Trim() : u.BusinessName!,
                        Address = u.Address ?? string.Empty,
                        Latitude = u.Latitude,
                        Longitude = u.Longitude,
                        ItemsCount = g.Sum(x => x.Quantity)
                    };
                }).ToList();

            bool hasCourier = courierLat.HasValue && courierLng.HasValue;
            // The store to measure from: the nearest one to the courier, else the first one with a position.
            var located = stops.Where(s => s.Latitude != 0 || s.Longitude != 0).ToList();
            (double Lat, double Lng)? pickup = null;
            if (located.Count > 0)
            {
                var s = hasCourier
                    ? located.OrderBy(x => OrderFlow.DistanceKm(courierLat!.Value, courierLng!.Value, x.Latitude, x.Longitude)).First()
                    : located[0];
                pickup = (s.Latitude, s.Longitude);
            }
            else if (o.PickupLatitude != 0 || o.PickupLongitude != 0)
            {
                pickup = (o.PickupLatitude, o.PickupLongitude);
            }
            (double Lat, double Lng)? dropoff = (o.DropoffLatitude != 0 || o.DropoffLongitude != 0)
                ? (o.DropoffLatitude, o.DropoffLongitude) : null;

            string pickupDistance = pickup.HasValue && hasCourier
                ? $"يبعد {OrderFlow.DistanceKm(courierLat!.Value, courierLng!.Value, pickup.Value.Lat, pickup.Value.Lng):F1} كم عن موقعك"
                : string.Empty;
            string tripDistance = pickup.HasValue && dropoff.HasValue
                ? $"{OrderFlow.DistanceKm(pickup.Value.Lat, pickup.Value.Lng, dropoff.Value.Lat, dropoff.Value.Lng):F1} كم رحلة التوصيل"
                : string.Empty;

            return new DeliveryOrderDto
            {
                OrderId = o.Id,
                OrderNumber = $"ORD-{o.Id}",
                TotalPrice = (decimal)o.TotalPrice,
                Currency = "ج.م",
                OrderType = "توصيل طلبات متجر",
                CategoryBadge = "نشط",

                PickupTitle = stops.Count == 0 ? "مستودع برواح / المطعم"
                    : stops.Count == 1 ? stops[0].StoreName
                    : $"{stops.Count} محلات: " + string.Join("، ", stops.Select(s => s.StoreName)),
                PickupAddress = stops.Count == 0 ? "جاهز للاستلام الفوري"
                    : stops.Count == 1 ? (string.IsNullOrWhiteSpace(stops[0].Address) ? "جاهز للاستلام الفوري" : stops[0].Address)
                    : "استلام من أكثر من محل",
                Stops = stops,
                PickupDistance = pickupDistance,

                DropoffTitle = "عنوان العميل",
                DropoffAddress = string.IsNullOrWhiteSpace(o.DropoffAddress) ? "التوصيل للعنوان المحدد" : o.DropoffAddress!,
                DropoffLatitude = dropoff?.Lat,
                DropoffLongitude = dropoff?.Lng,
                DropoffDistance = tripDistance,

                PaymentNote = o.PaymentMethod == PaymentMethod.Cash ? $"نقداً عند الاستلام • تحصيل: {o.TotalPrice} ج.م" : "مسبق الدفع",
                ActionButtonText = "قبول الطلب والتوجه للاستلام"
            };
        }

        [Authorize(Roles = "Delivery")]
        [HttpGet("AvailableOrders")]
        public async Task<IActionResult> GetAvailableOrders([FromQuery] double? lat = null, [FromQuery] double? lng = null)
        {
            try
            {
                var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                // A captain who switched the radar off sees nothing.
                var unavailable = await _context.Users.AsNoTracking().Where(u => u.Id == me).Select(u => u.IsUnavailable).FirstOrDefaultAsync();
                if (unavailable)
                    return Ok(new { status = "success", activeCount = 0, data = new List<DeliveryOrderDto>() });

                var orders = await _context.Orders
                    .Where(o => o.DeliveryId == null
                                && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped))
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.User)
                    .ToListAsync();

                // Food stores (restaurants, groceries, home kitchens) must accept before a courier
                // sees the order; other items can be taken right away.
                orders = orders.Where(OrderFlow.IsReadyForCouriers).ToList();

                // The moment an order first shows on a radar starts the clock for the captains' response time.
                var firstSeen = orders.Where(o => o.FirstOfferedAt == null).ToList();
                if (firstSeen.Count > 0)
                {
                    foreach (var o in firstSeen) o.FirstOfferedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                var availableOrders = orders.Select(o => BuildOrderDto(o, lat, lng)).ToList();
                return Ok(new { status = "success", activeCount = availableOrders.Count, data = availableOrders });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        // The order the captain is on right now (so a new phone or a cleared app picks the trip back up).
        [Authorize(Roles = "Delivery")]
        [HttpGet("ActiveOrder")]
        public async Task<IActionResult> GetActiveOrder([FromQuery] double? lat = null, [FromQuery] double? lng = null)
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _context.Orders.AsNoTracking()
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.User)
                .Where(o => o.DeliveryId == me
                            && (o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.OutForDelivery))
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();
            if (order == null) return Ok(new { status = "success", data = (object?)null });

            var picked = order.OrderItems
                .Where(i => i.PickedUpAt != null && i.Product != null)
                .Select(i => i.Product!.UserId).Distinct().ToList();
            return Ok(new
            {
                status = "success",
                data = BuildOrderDto(order, lat, lng),
                pickedUp = order.Status == OrderStatus.OutForDelivery,
                pickedMerchantIds = picked
            });
        }

        public class AvailabilityDto
        {
            public bool Available { get; set; }
        }

        [Authorize(Roles = "Delivery")]
        [HttpGet("Availability")]
        public async Task<IActionResult> GetAvailability()
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var unavailable = await _context.Users.AsNoTracking().Where(u => u.Id == me).Select(u => u.IsUnavailable).FirstOrDefaultAsync();
            return Ok(new { status = "success", data = new { available = !unavailable } });
        }

        [Authorize(Roles = "Delivery")]
        [HttpPut("Availability")]
        public async Task<IActionResult> SetAvailability([FromBody] AvailabilityDto dto)
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var captain = await _context.Users.FirstOrDefaultAsync(u => u.Id == me);
            if (captain == null) return NotFound(new { status = "error", message = "الكابتن غير موجود" });
            captain.IsUnavailable = !dto.Available;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", data = new { available = dto.Available } });
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
                    .Include(o => o.OrderItems).ThenInclude(i => i.Product).ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.DeliveryId == null
                        && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped));

                if (order == null || !OrderFlow.IsReadyForCouriers(order))
                {
                    return BadRequest(new { status = "error", message = "عذراً، هذا الطلب تم قبوله مسبقاً بواسطة مندوب آخر أو لم يعد متاحاً." });
                }

                var captain = await _userManager.FindByIdAsync(deliveryId.ToString());

                order.DeliveryId = deliveryId;
                order.FirstOfferedAt ??= DateTime.UtcNow;
                order.CourierAcceptedAt = DateTime.UtcNow;
                if (order.Status == OrderStatus.Pending) order.Status = OrderStatus.Processing;

                await _context.SaveChangesAsync();

                try { await _notifications.SendNotificationAsync(new SendNotification { UserId = order.UserId, Title = "تم قبول طلبك 🛵", Message = $"المندوب {($"{captain?.FirstName} {captain?.LastName}").Trim()} قبل طلبك #{order.Id} وفي طريقه للمحل." }); } catch { }

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
                        rating = captain?.Rating ?? 0,
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
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء قبول الطلب" });
            }
        }

        // تأكيد استلام الطلب من محل (أو من كل المحلات لو merchantId مش مبعوت)
        [Authorize(Roles = "Delivery")]
        [HttpPost("orders/{orderId}/pickup")]
        public async Task<IActionResult> ConfirmPickup(int orderId, [FromQuery] int? merchantId = null)
        {
            var deliveryId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.DeliveryId == deliveryId);

            if (order == null) return NotFound(new { status = "error", message = "الطلب غير موجود أو غير مسجل لك" });
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                return BadRequest(new { status = "error", message = "الطلب منتهي بالفعل" });

            // Items of stores that rejected their part are not collected.
            var active = order.OrderItems.Where(i => i.MerchantStatus != OrderStatus.Cancelled).ToList();
            var toPick = merchantId.HasValue
                ? active.Where(i => i.Product != null && i.Product.UserId == merchantId.Value).ToList()
                : active;

            if (toPick.Count == 0)
                return NotFound(new { status = "error", message = "لا توجد أصناف من هذا المحل في الطلب" });

            foreach (var item in toPick)
                if (item.PickedUpAt == null) item.PickedUpAt = DateTime.UtcNow;

            int remaining = active
                .Where(i => i.PickedUpAt == null && i.Product != null)
                .Select(i => i.Product.UserId).Distinct().Count();
            bool allPickedUp = remaining == 0;

            if (allPickedUp) order.Status = OrderStatus.OutForDelivery;
            await _context.SaveChangesAsync();

            if (allPickedUp)
            {
                // إشعار العميل فوراً بتحديث الشاشة إلى مرحلة "في الطريق إليك"
                await _hubContext.Clients.Group($"order_{orderId}").SendAsync("ReceiveOrderStatusUpdate", new
                {
                    status = order.Status.ToString(),
                    message = "تم استلام الطلب من المحلات وهو في طريقه إليك الآن"
                });
            }

            return Ok(new
            {
                status = "success",
                allPickedUp,
                remainingStops = remaining,
                message = allPickedUp
                    ? "تم استلام كل الطلب، جاري التوجه للعميل"
                    : $"تم الاستلام، متبقي {remaining} محل"
            });
        }

        // تأكيد تسليم الطلب للعميل
        [Authorize(Roles = "Delivery")]
        [HttpPost("orders/{orderId}/deliver")]
        public async Task<IActionResult> ConfirmDelivered(int orderId)
        {
            var deliveryId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.DeliveryId == deliveryId);

            if (order == null) return NotFound(new { status = "error", message = "الطلب غير موجود أو غير مسجل لك" });
            if (order.Status != OrderStatus.OutForDelivery)
                return BadRequest(new { status = "error", message = "استلم الطلب من كل المحلات أولاً" });

            order.Status = OrderStatus.Delivered;
            order.DeliveredAt = DateTime.UtcNow;
            order.ConditionStaus = "تم التسليم";
            await _context.SaveChangesAsync();
            CaptainPositions.TryRemove(orderId, out _);
            try { await _notifications.SendNotificationAsync(new SendNotification { UserId = order.UserId, Title = "تم تسليم طلبك ✅", Message = $"تم تسليم طلبك #{order.Id}، بالهنا والشفا." }); } catch { }

            await _hubContext.Clients.Group($"order_{orderId}").SendAsync("ReceiveOrderStatusUpdate", new
            {
                status = order.Status.ToString(),
                message = "تم تسليم طلبك، بالهنا والشفا"
            });

            return Ok(new { status = "success", message = "تم تسليم الطلب" });
        }

        // تحديث إحداثيات موقع الكابتن لحظياً على الخريطة
        [Authorize(Roles = "Delivery")]
        [HttpPost("orders/{orderId}/location")]
        public async Task<IActionResult> UpdateCaptainLocation(int orderId, [FromBody] LocationDto locDto)
        {
            CaptainPositions[orderId] = (locDto.Latitude, locDto.Longitude);
            await _hubContext.Clients.Group($"order_{orderId}").SendAsync("ReceiveLocationUpdate", locDto.Latitude, locDto.Longitude);
            return Ok(new { status = "success" });
        }

        #endregion
        #region Vehicle, stepping back and rating

        public class VehicleDto
        {
            public string? PlateNumber { get; set; }
            public string? LicenseExpiry { get; set; }
        }

        // The captain fills in the plate and the driver's-license expiry shown on the profile.
        [Authorize(Roles = "Delivery")]
        [HttpPut("CaptainVehicle")]
        public async Task<IActionResult> UpdateVehicle([FromBody] VehicleDto dto)
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var captain = await _context.Users.FirstOrDefaultAsync(u => u.Id == me);
            if (captain == null) return NotFound(new { status = "error", message = "الكابتن غير موجود" });

            DateTime? expiry = null;
            if (!string.IsNullOrWhiteSpace(dto.LicenseExpiry))
            {
                if (!DateTime.TryParse(dto.LicenseExpiry, System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var parsed))
                    return BadRequest(new { status = "error", message = "تاريخ انتهاء الرخصة غير صالح" });
                expiry = parsed.Date;
            }
            var plate = dto.PlateNumber?.Trim();
            captain.VehiclePlateNumber = string.IsNullOrEmpty(plate) ? null : (plate.Length > 30 ? plate[..30] : plate);
            if (captain.DrivingLicenseExpiry != expiry) captain.LicenseReminderStage = 0;
            captain.DrivingLicenseExpiry = expiry;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "تم تحديث بيانات المركبة" });
        }

        // The captain steps back from an order he accepted, before collecting anything: it goes back to the radar.
        [Authorize(Roles = "Delivery")]
        [HttpPost("orders/{orderId}/release")]
        public async Task<IActionResult> ReleaseOrder(int orderId)
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.DeliveryId == me);
            if (order == null) return NotFound(new { status = "error", message = "الطلب غير موجود أو غير مسجل لك" });

            if (order.Status == OrderStatus.OutForDelivery || order.Status == OrderStatus.Delivered
                || order.Status == OrderStatus.Cancelled || order.OrderItems.Any(i => i.PickedUpAt != null))
                return BadRequest(new { status = "error", message = "لا يمكنك الاعتذار عن هذا الطلب بعد بدء الاستلام" });

            order.DeliveryId = null;
            order.CourierAcceptedAt = null;
            order.FirstOfferedAt = DateTime.UtcNow;
            var captain = await _context.Users.FirstOrDefaultAsync(u => u.Id == me);
            if (captain != null) captain.ReleasedOrdersCount++;
            await _context.SaveChangesAsync();
            CaptainPositions.TryRemove(orderId, out _);

            try
            {
                await _notifications.SendNotificationAsync(new SendNotification
                {
                    UserId = order.UserId,
                    Title = "تعذر على المندوب إكمال طلبك 🛵",
                    Message = $"تعذر على المندوب إكمال طلبك #{order.Id}. نبحث لك عن مندوب آخر."
                });
            }
            catch { /* best effort */ }

            await _hubContext.Clients.Group($"order_{orderId}").SendAsync("ReceiveOrderStatusUpdate", new
            {
                status = order.Status.ToString(),
                message = "نبحث لك عن مندوب آخر"
            });
            return Ok(new { status = "success", message = "تم الاعتذار عن الطلب" });
        }

        public class RateCourierDto
        {
            public int Rating { get; set; }
            public string? Review { get; set; }
        }

        // The customer rates the courier once, after the order was delivered.
        [Authorize]
        [HttpPost("orders/{orderId}/rate")]
        public async Task<IActionResult> RateCourier(int orderId, [FromBody] RateCourierDto dto)
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { status = "error", message = "التقييم يجب أن يكون من 1 إلى 5" });

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == me);
            if (order == null) return NotFound(new { status = "error", message = "الطلب غير موجود" });
            if (order.Status != OrderStatus.Delivered || order.DeliveryId == null)
                return BadRequest(new { status = "error", message = "لا يمكن تقييم هذا الطلب" });
            if (order.CourierRating != null)
                return BadRequest(new { status = "error", message = "تم تقييم هذا الطلب من قبل" });

            var review = dto.Review?.Trim();
            order.CourierRating = dto.Rating;
            order.CourierReview = string.IsNullOrEmpty(review) ? null : (review.Length > 500 ? review[..500] : review);
            await _context.SaveChangesAsync();

            // The captain's rating is the average of what customers gave.
            var captainId = order.DeliveryId.Value;
            var ratings = await _context.Orders.Where(o => o.DeliveryId == captainId && o.CourierRating != null)
                .Select(o => o.CourierRating!.Value).ToListAsync();
            var captain = await _context.Users.FirstOrDefaultAsync(u => u.Id == captainId);
            if (captain != null && ratings.Count > 0)
            {
                captain.Rating = Math.Round(ratings.Average(), 1);
                await _context.SaveChangesAsync();
            }

            try
            {
                await _notifications.SendNotificationAsync(new SendNotification
                {
                    UserId = captainId,
                    Title = "قيّمك العميل ⭐",
                    Message = $"قيّمك العميل {dto.Rating} من 5 على الطلب #{order.Id}."
                });
            }
            catch { /* best effort */ }

            return Ok(new { status = "success", message = "تم تسجيل تقييمك، شكرًا لك" });
        }

        #endregion

        #region Trip history, documents and problem reports

        // Every order the captain delivered, newest first, a page at a time.
        [Authorize(Roles = "Delivery")]
        [HttpGet("Trips")]
        public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 50);
            var share = await GetCourierShareAsync();

            var orders = await _context.Orders.AsNoTracking()
                .Include(o => o.OrderItems).ThenInclude(i => i.Product).ThenInclude(p => p.User)
                .Where(o => o.DeliveryId == me && o.Status == OrderStatus.Delivered)
                .OrderByDescending(o => o.DeliveredAt ?? o.CreatedAt).ThenByDescending(o => o.Id)
                .Skip((page - 1) * pageSize).Take(pageSize + 1)
                .ToListAsync();

            var data = orders.Take(pageSize).Select(o =>
            {
                var stores = o.OrderItems
                    .Where(i => i.Product?.User != null && i.MerchantStatus != OrderStatus.Cancelled)
                    .Select(i => string.IsNullOrWhiteSpace(i.Product!.User!.BusinessName)
                        ? $"{i.Product.User.FirstName} {i.Product.User.LastName}".Trim()
                        : i.Product.User.BusinessName!)
                    .Distinct().ToList();
                var at = o.DeliveredAt ?? o.CreatedAt;
                return new
                {
                    orderId = o.Id,
                    orderNumber = $"ORD-{o.Id}",
                    title = stores.Count == 0 ? $"طلب توصيل #{o.Id}"
                        : stores.Count == 1 ? stores[0]
                        : $"{stores.Count} محلات: " + string.Join("، ", stores),
                    total = (decimal)o.TotalPrice,
                    earning = Math.Round((decimal)o.TotalPrice * share, 2),
                    cash = o.PaymentMethod == PaymentMethod.Cash,
                    dateText = at.ToString("dd/MM/yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture),
                    rating = o.CourierRating
                };
            }).ToList();

            return Ok(new { status = "success", data, hasMore = orders.Count > pageSize });
        }

        // Replaces one of the captain's pictures: national, license, criminal or vehicle.
        [Authorize(Roles = "Delivery")]
        [HttpPost("Documents/{kind}")]
        public async Task<IActionResult> UploadDocument(string kind, IFormFile file)
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            kind = (kind ?? string.Empty).ToLowerInvariant();
            if (kind != "national" && kind != "license" && kind != "criminal" && kind != "vehicle")
                return BadRequest(new { status = "error", message = "نوع الوثيقة غير صحيح" });
            if (file == null || file.Length == 0)
                return BadRequest(new { status = "error", message = "الصورة مطلوبة" });
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { status = "error", message = "حجم الصورة كبير جدًا" });

            var captain = await _context.Users.FirstOrDefaultAsync(u => u.Id == me);
            if (captain == null) return NotFound(new { status = "error", message = "الكابتن غير موجود" });

            var upload = await _imageService.UploadImageAsync(file);
            var url = upload?.SecureUrl?.ToString();
            if (string.IsNullOrEmpty(url))
                return StatusCode(500, new { status = "error", message = "تعذر رفع الصورة، حاول مرة أخرى" });

            switch (kind)
            {
                case "national": captain.NationalIdImageUrl = url; break;
                case "license": captain.DrivingLicenseImageUrl = url; break;
                case "criminal": captain.CriminalRecordImageUrl = url; break;
                default: captain.VehicleImageUrl = url; break;
            }
            await _context.SaveChangesAsync();

            // The admin looks at the new picture.
            try
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                foreach (var admin in admins)
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = admin.Id,
                        Title = "وثيقة كابتن جديدة 📄",
                        Message = $"حدّث الكابتن {captain.FirstName} {captain.LastName} إحدى وثائقه. راجعها."
                    });
            }
            catch { /* best effort */ }

            return Ok(new { status = "success", message = "تم رفع الوثيقة", data = new { url } });
        }

        public class OrderIssueDto
        {
            public string? Reason { get; set; }
            public string? Note { get; set; }
        }

        private static readonly Dictionary<string, string> IssueReasons = new()
        {
            ["customer_unreachable"] = "العميل لا يرد",
            ["wrong_address"] = "العنوان غير صحيح",
            ["store_closed"] = "المتجر مغلق",
            ["items_missing"] = "الطلب ناقص",
            ["other"] = "مشكلة أخرى"
        };

        // The captain reports a problem on the order he is delivering; it does not count against him.
        [Authorize(Roles = "Delivery")]
        [HttpPost("orders/{orderId}/issue")]
        public async Task<IActionResult> ReportIssue(int orderId, [FromBody] OrderIssueDto dto)
        {
            var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId && o.DeliveryId == me);
            if (order == null) return NotFound(new { status = "error", message = "الطلب غير موجود أو غير مسجل لك" });

            var reason = dto.Reason != null && IssueReasons.ContainsKey(dto.Reason) ? dto.Reason : "other";
            var note = dto.Note?.Trim();
            if (reason == "other" && string.IsNullOrEmpty(note))
                return BadRequest(new { status = "error", message = "اكتب وصف المشكلة" });

            _context.OrderIssues.Add(new OrderIssue
            {
                OrderId = orderId,
                ReporterId = me,
                Reason = reason,
                Note = string.IsNullOrEmpty(note) ? null : (note.Length > 500 ? note[..500] : note),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            try
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                foreach (var admin in admins)
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = admin.Id,
                        Title = "بلاغ مشكلة من مندوب ⚠️",
                        Message = $"بلاغ على الطلب #{orderId}: {IssueReasons[reason]}"
                    });

                // The customer can fix these two himself.
                if (reason == "customer_unreachable" || reason == "wrong_address")
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = order.UserId,
                        Title = "المندوب يحاول الوصول إليك 📞",
                        Message = $"المندوب لم يتمكن من الوصول إليك بخصوص طلبك #{orderId}. تأكد من العنوان وارد على الهاتف."
                    });
            }
            catch { /* best effort */ }

            return Ok(new { status = "success", message = "تم إرسال البلاغ للإدارة" });
        }

        #endregion

        #region Wallet & Earnings (المحفظة والأرباح وسحب الأموال)

        /// <summary>The captain's share of an order's total, from the settings (15% when unset).</summary>
        private async Task<decimal> GetCourierShareAsync()
        {
            var percent = (await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync())?.CourierSharePercentage ?? 0m;
            return (percent > 0 ? percent : 15m) / 100m;
        }
        private const string WithdrawType = "CourierWithdrawal";
        private const string DepositType = "CourierCashDeposit";

        private sealed record WalletFigures(
            List<Order> Delivered, decimal Share, decimal TotalEarnings, decimal Withdrawn, decimal CashCollected, decimal CashDeposited,
            List<Transaction> Moves)
        {
            public decimal Available => Math.Max(0, TotalEarnings - Withdrawn);
            public decimal CashInHand => Math.Max(0, CashCollected - CashDeposited);
        }

        private async Task<WalletFigures> LoadWalletAsync(int captainId)
        {
            var delivered = await _context.Orders.AsNoTracking()
                .Where(o => o.DeliveryId == captainId && o.Status == OrderStatus.Delivered)
                .OrderByDescending(o => o.Id)
                .ToListAsync();
            var moves = await _context.Transactions.AsNoTracking()
                .Where(t => t.UserId == captainId && (t.Type == WithdrawType || t.Type == DepositType))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            var share = await GetCourierShareAsync();
            return new WalletFigures(
                delivered,
                share,
                delivered.Sum(o => (decimal)o.TotalPrice * share),
                moves.Where(t => t.Type == WithdrawType).Sum(t => t.Amount),
                delivered.Where(o => o.PaymentMethod == PaymentMethod.Cash).Sum(o => (decimal)o.TotalPrice),
                moves.Where(t => t.Type == DepositType).Sum(t => t.Amount),
                moves);
        }

        [Authorize(Roles = "Delivery")]
        [HttpGet("Wallet")]
        public async Task<IActionResult> GetCaptainWallet()
        {
            try
            {
                var captainId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var w = await LoadWalletAsync(captainId);

                DateTime When(Order o) => o.DeliveredAt ?? o.CreatedAt;
                decimal Earn(IEnumerable<Order> list) => list.Sum(o => (decimal)o.TotalPrice * w.Share);
                var today = DateTime.UtcNow.Date;
                var todayOrders = w.Delivered.Where(o => When(o).Date == today).ToList();
                var weekOrders = w.Delivered.Where(o => When(o) >= DateTime.UtcNow.AddDays(-7)).ToList();
                var monthOrders = w.Delivered.Where(o => When(o) >= DateTime.UtcNow.AddMonths(-1)).ToList();

                // Hours on the road today: from taking an order to delivering it.
                var hours = todayOrders.Where(o => o.CourierAcceptedAt != null && o.DeliveredAt != null)
                    .Sum(o => Math.Max(0, (o.DeliveredAt!.Value - o.CourierAcceptedAt!.Value).TotalHours));

                var rows = new List<(DateTime At, TransactionDto Row)>();
                rows.AddRange(w.Delivered.Take(10).Select(o => (When(o), new TransactionDto
                {
                    Id = o.Id,
                    Title = $"طلب توصيل #{o.Id}",
                    Subtitle = o.PaymentMethod == PaymentMethod.Cash ? $"كاش مستلم {o.TotalPrice} ج.م" : "دفع إلكتروني محفظة",
                    Amount = (decimal)o.TotalPrice * w.Share,
                    Type = "Earning",
                    DateText = When(o).ToString("dd/MM hh:mm tt"),
                    IsPositive = true
                })));
                rows.AddRange(w.Moves.Take(10).Select(t => (t.CreatedAt, new TransactionDto
                {
                    Id = -t.Id,
                    Title = t.Type == WithdrawType ? "سحب أرباح" : "توريد كاش",
                    Subtitle = t.Description,
                    Amount = t.Amount,
                    Type = t.Type == WithdrawType ? "Withdrawal" : "Deposit",
                    DateText = t.CreatedAt.ToString("dd/MM hh:mm tt"),
                    IsPositive = false
                })));

                var dto = new WalletResponseDto
                {
                    TodayEarnings = Earn(todayOrders),
                    WeeklyEarnings = Earn(weekOrders),
                    MonthlyEarnings = Earn(monthOrders),
                    CompletedOrdersCount = w.Delivered.Count,
                    DrivingHours = $"{Math.Round(hours, 1)} ساعة",
                    AvgOrderEarnings = w.Delivered.Count > 0 ? Math.Round(w.TotalEarnings / w.Delivered.Count, 2) : 0,
                    CashInHand = w.CashInHand,
                    DigitalBalance = w.Available,
                    RecentTransactions = rows.OrderByDescending(r => r.At).Take(10).Select(r => r.Row).ToList()
                };
                return Ok(new { status = "success", data = dto });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        // Records a withdrawal request against the captain's balance; the admin pays it out.
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
                decimal companyFee = settings?.WithdrawalFeeFixed ?? 0m;
                decimal minimum = settings?.MinWithdrawalAmount ?? 0m;

                if (model.Amount <= companyFee)
                    return BadRequest(new { status = "error", message = "مبلغ السحب يجب أن يكون أكبر من رسوم السحب المقرر" });
                if (model.Amount < minimum)
                    return BadRequest(new { status = "error", message = "مبلغ السحب أقل من الحد الأدنى" });

                var w = await LoadWalletAsync(captainId);
                if (model.Amount > w.Available)
                    return BadRequest(new { status = "error", message = "الرصيد المتاح لا يكفي لهذا المبلغ" });

                var account = (model.DestinationAccount ?? string.Empty).Trim();
                var tail = account.Length > 4 ? account[^4..] : account;
                _context.Transactions.Add(new Transaction
                {
                    UserId = captainId,
                    Amount = model.Amount,
                    Type = WithdrawType,
                    Description = $"{model.PaymentMethod} {tail}".Trim(),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = "success",
                    message = "تم تسجيل طلب السحب، وسيتم التحويل بعد مراجعة الإدارة.",
                    data = new
                    {
                        requestedAmount = model.Amount,
                        companyFee,
                        netReceived = model.Amount - companyFee,
                        payoutMethod = model.PaymentMethod,
                        requestedAt = DateTime.UtcNow
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        // Records that the captain handed in cash he collected.
        [Authorize(Roles = "Delivery")]
        [HttpPost("DepositCash")]
        public async Task<IActionResult> DepositCash([FromBody] DepositeCashDto model)
        {
            try
            {
                var captainId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                if (model.Amount <= 0)
                    return BadRequest(new { status = "error", message = "المبلغ المراد توريده غير صالح" });

                var w = await LoadWalletAsync(captainId);
                if (model.Amount > w.CashInHand)
                    return BadRequest(new { status = "error", message = "المبلغ أكبر من الكاش الموجود معك" });

                _context.Transactions.Add(new Transaction
                {
                    UserId = captainId,
                    Amount = model.Amount,
                    Type = DepositType,
                    Description = (model.TransferMethod ?? string.Empty).Trim(),
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = "success",
                    message = "تم تسجيل توريد الكاش، وفي انتظار مراجعة الإدارة.",
                    data = new { depositedAmount = model.Amount, method = model.TransferMethod }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
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