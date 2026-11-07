using Berwah.BL.Service;
using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Berwah.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notificationService;
        private readonly GeoZoneService _geo;

        public OrderController(ApplicationDbContext context, UserManager<User> userManager, INotificationService notificationService, GeoZoneService geo)
        {
            _geo = geo;
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Where the delivery spot is against every store of the order: the same city, another
        /// city (allowed, costs more), another governorate or another country (not allowed).
        /// Stores whose place or the spot itself is unknown never block.
        /// </summary>
        private async Task<(DeliveryZone Zone, double Km)> EvaluateZoneAsync(IEnumerable<CartItemDto>? items, double lat, double lon)
        {
            if (items == null || (lat == 0 && lon == 0)) return (DeliveryZone.Unknown, 0);
            var ids = items.Select(i => i.ProductId).Distinct().ToList();
            var stores = await _context.Products
                .Where(p => ids.Contains(p.Id) && p.User != null && p.User.Latitude != 0 && p.User.Longitude != 0)
                .Select(p => new { p.UserId, p.User!.Latitude, p.User.Longitude })
                .Distinct()
                .ToListAsync();
            if (stores.Count == 0) return (DeliveryZone.Unknown, 0);

            // The farthest store sets the distance (the courier visits them all).
            var km = stores.Max(s => GeoZoneService.DistanceKm(s.Latitude, s.Longitude, lat, lon));

            var dropoff = await _geo.ResolveAsync(lat, lon);
            if (dropoff == null) return (DeliveryZone.Unknown, km);

            var zones = new List<DeliveryZone>();
            foreach (var s in stores)
                zones.Add(GeoZoneService.Compare(await _geo.ResolveAsync(s.Latitude, s.Longitude), dropoff));
            return (GeoZoneService.Worst(zones), km);
        }

        private static string BlockedMessage(DeliveryZone zone) => zone == DeliveryZone.BlockedCountry
            ? "لا يمكن إتمام الطلب لاختلاف بلد الإقامة، يمكنك الطلب فقط إذا كان التوصيل داخل نفس بلد المتجر"
            : "لا يمكن إتمام الطلب لأن المتجر في محافظة مختلفة عن عنوان التوصيل";

        [HttpPost("Summary")]
        public async Task<IActionResult> GetOrderSummary([FromBody] CheckoutDto model)
        {
            try
            {
                decimal subTotal = 0;
                foreach (var item in model.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        subTotal += product.Price * item.Quantity;
                    }
                }

                var (zone, km) = await EvaluateZoneAsync(model.Items, model.UserLatitude, model.UserLongitude);
                var extraFee = GeoZoneService.ExtraFeeFor(km);
                decimal deliveryFee = 30 + extraFee;
                decimal discount = 0;

                var summary = new OrderSummeryDto
                {
                    SubTotal = subTotal,
                    DeliveryFee = deliveryFee,
                    Discount = discount,
                    Total = subTotal + deliveryFee - discount,
                    DeliveryZone = GeoZoneService.Code(zone),
                    DistanceKm = Math.Round(km, 1),
                    ExtraDeliveryFee = extraFee,
                    FreeDistanceKm = GeoZoneService.FreeDistanceKm,
                    PricePerExtraKm = GeoZoneService.PricePerExtraKm
                };

                return Ok(new { status = "success", data = summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        #region 2. Checkout (إتمام الطلب وفحص المنتجات المستعملة)
        [Authorize]
        [HttpPost("Checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto model)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdString, out int userId))
                    return Unauthorized();

                if (model.Items == null || !model.Items.Any())
                    return BadRequest(new { status = "error", message = "السلة فارغة" });

                // An order only goes to the store's own governorate (another city of it costs
                // more), and never to another country.
                var (zone, km) = await EvaluateZoneAsync(model.Items, model.UserLatitude, model.UserLongitude);
                if (zone == DeliveryZone.BlockedCountry || zone == DeliveryZone.BlockedGovernorate)
                    return BadRequest(new { status = "error", code = "delivery_blocked", zone = GeoZoneService.Code(zone), message = BlockedMessage(zone) });

                decimal subTotal = 0;
                var orderItems = new List<OrderItem>();
                bool hasUsedProducts = false;

                // سنأخذ إحداثيات الاستلام (Pickup) من أول منتج أو تاجر في السلة كافتراضي
                double pickupLat = 30.0444; // قيمة افتراضية لحين توفر إحداثيات البائع
                double pickupLon = 31.2357;
                // The cart can hold items of several stores: it is still one order. The
                // pickup point is the first store that has coordinates; the courier sees
                // every store as a stop (Delivery/AvailableOrders -> stops).
                bool pickupFromStore = false;

                foreach (var cartItem in model.Items)
                {
                    var product = await _context.Products
                        .Include(p => p.User) // جلب بيانات التاجر/المتجر المالك للمنتج
                        .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

                    if (product == null || product.Stock < cartItem.Quantity)
                        return BadRequest(new { status = "error", message = "المنتج غير موجود أو الكمية غير متوفرة" });

                    if (product.User != null)
                    {
                        // A closed store (the merchant toggled it off) takes no orders.
                        if (product.User.UserType == UserType.Merchant && (product.User.IsFrozen || product.User.IsPaused))
                            return BadRequest(new { status = "error", message = "المتجر مغلق حاليًا ولا يستقبل طلبات" });

                        if (!pickupFromStore && product.User.Latitude != 0 && product.User.Longitude != 0)
                        {
                            pickupLat = product.User.Latitude;
                            pickupLon = product.User.Longitude;
                            pickupFromStore = true;
                        }
                    }

                    // فحص حالة المنتج إذا كان مستعملاً (Used)
                    if (product.Condition == ProductCondition.Used)
                    {
                        hasUsedProducts = true;
                    }

                    subTotal += product.Price * cartItem.Quantity;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = cartItem.Quantity,
                        Price = product.Price,
                        MerchantStatus = OrderStatus.Pending
                    });

                    product.Stock -= cartItem.Quantity;
                }

                decimal deliveryFee = 30 + GeoZoneService.ExtraFeeFor(km);
                decimal finalTotal = subTotal + deliveryFee;

                // What was bought leaves the server cart in the same save as the order.
                var boughtIds = model.Items.Select(i => i.ProductId).ToList();
                var cartRows = await _context.CartItems
                    .Where(c => c.UserId == userId && boughtIds.Contains(c.ProductId))
                    .ToListAsync();
                _context.CartItems.RemoveRange(cartRows);

                var order = new Order
                {
                    UserId = userId,
                    TotalPrice = (float)finalTotal,
                    PaymentMethod = model.PaymentMethod,
                    CreatedAt = DateTime.UtcNow,

                    PickupLatitude = pickupLat,
                    PickupLongitude = pickupLon,
                    DropoffLatitude = model.UserLatitude,   
                    DropoffLongitude = model.UserLongitude,
                    DropoffAddress = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),

                    OrderItems = orderItems
                };

                if (hasUsedProducts)
                {
                    order.Status = OrderStatus.PendingAdminApproval;
                    order.ConditionStaus = "Pending Admin Approval";

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    var admins = await _userManager.GetUsersInRoleAsync("Admin");
                    foreach (var admin in admins)
                    {
                        await _notificationService.SendNotificationAsync(new SendNotification
                        {
                            UserId = admin.Id,
                            Title = "طلب منتج مستعمل جديد",
                            Message = $"يوجد طلب جديد رقم #{order.Id} يحتوي على منتجات مستعملة ويتطلب موافقتك."
                        });
                    }

                    await SafeNotifyAsync(order.UserId, "تم استلام طلبك", $"طلبك رقم #{order.Id} قيد مراجعة الإدارة لاحتوائه على منتجات مستعملة.");

                    return Ok(new
                    {
                        status = "success",
                        requiresApproval = true,
                        message = "يحتوي طلبك على منتجات مستعملة، سيتم مراجعة الطلب من قبل الإدارة قريباً.",
                        orderId = order.Id
                    });
                }
                else
                {
                    order.Status = OrderStatus.Pending;
                    order.ConditionStaus = "New - Confirmed";

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    await SafeNotifyAsync(order.UserId, "تم استلام طلبك ✅", $"تم إتمام طلبك رقم #{order.Id} وجاري تجهيزه.");
                    await NotifyStoresAndCouriersAsync(order);

                    return Ok(new
                    {
                        status = "success",
                        requiresApproval = false,
                        message = "تم إتمام الطلب بنجاح",
                        orderId = order.Id
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ أثناء إتمام الطلب", details = ex.Message });
            }
        }
        #endregion

        #region 3. Order Confirmation (صفحة تأكيد الطلب)
        [Authorize]
        [HttpGet("Confirmation/{orderId}")]
        public async Task<IActionResult> GetOrderConfirmation(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.Delivery)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return NotFound(new { status = "error", message = "الطلب غير موجود" });

                var steps = new List<OrderTrackingStepDto>
                {
                    new() { Title = "تم تأكيد الطلب", Subtitle = order.CreatedAt.ToString("hh:mm tt"), IsCompleted = true, IsCurrent = false },
                    new() { Title = "جاري التجهيز والخرج الآمن", Subtitle = "شحن موجّه داخل العبوة وتحت الرقابة", IsCompleted = order.Status >= OrderStatus.Processing, IsCurrent = order.Status == OrderStatus.Processing },
                    new() { Title = "في الطريق إليك", Subtitle = "سريع جداً", IsCompleted = order.Status >= OrderStatus.Shipped, IsCurrent = order.Status == OrderStatus.Shipped || order.Status == OrderStatus.OutForDelivery },
                    new() { Title = "تم التسليم والاستلام", Subtitle = "شكراً لاختيارك برواح", IsCompleted = order.Status == OrderStatus.Delivered, IsCurrent = order.Status == OrderStatus.Delivered }
                };

                DeliveryMapDto? deliveryManDto = null;
                if (order.Delivery != null)
                {
                    deliveryManDto = new DeliveryMapDto
                    {
                        Id = order.Delivery.Id,
                        Name = order.Delivery.UserName ?? "مندوب برواح",
                        Rating = 4.9,
                        PhoneNumber = order.Delivery.PhoneNumber ?? "غير متوفر",
                        ImageUrl = order.Delivery.ProfileImageUrl ?? "https://res.cloudinary.com/..."
                    };
                }

                var confirmationDto = new OrderConfirmationDto
                {
                    OrderId = order.Id,
                    StatusText = order.DeliveryId == null ? "جاري البحث عن مندوب توصيل..." : "تم إسناد الطلب لمندوب التوصيل",
                    OrderNumber = $"BRW-{order.Id + 9180}",
                    DeliveryTimeEstimate = "35 - 45 دقيقة",
                    TrackingSteps = steps,
                    DeliveryMan = deliveryManDto,
                    PickupAddress = "العنوان المسجل للعميل",
                    TotalAmount = (decimal)order.TotalPrice,
                    PaymentMethodText = order.PaymentMethod.ToString()
                };

                return Ok(new { status = "success", data = confirmationDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
        #endregion

        #region 4. Live Tracking (خريطة التتبع الحي)
        [Authorize]
        [HttpGet("LiveTracking/{orderId}")]
        public async Task<IActionResult> GetLiveTracking(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Delivery)
                    .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return NotFound(new { status = "error", message = "الطلب غير موجود" });

                var cancelledSum = order.OrderItems.Where(i => i.MerchantStatus == OrderStatus.Cancelled).Sum(i => i.Price * i.Quantity);
                var allCancelled = order.OrderItems.Count > 0 && order.OrderItems.All(i => i.MerchantStatus == OrderStatus.Cancelled);
                var stageName = order.Status switch
                {
                    OrderStatus.Cancelled => "Cancelled",
                    OrderStatus.Delivered => "Delivered",
                    OrderStatus.OutForDelivery => "OnTheWay",
                    OrderStatus.Shipped => "PickedUp",
                    OrderStatus.Processing => "Preparing",
                    _ => "Pending"
                };

                var liveTrackingDto = new LiveTrackingDto
                {
                    OrderId = order.Id,
                    RemainingTime = "12 - 15 دقيقة",

                    // --- التعديل الهام هنا: استخدام الإحداثيات الفعلية المخزنة للطلب بدل الأرقام الثابتة ---
                    // Where the courier is now when he has sent a position; the pickup point until then.
                    DeliveryLatitude = DeliveryController.CaptainPositions.TryGetValue(order.Id, out var pos) ? pos.Lat : order.PickupLatitude,
                    DeliveryLongitude = DeliveryController.CaptainPositions.TryGetValue(order.Id, out var pos2) ? pos2.Lng : order.PickupLongitude,
                    UserLatitude = order.DropoffLatitude,       // موقع العميل الحقيقي
                    UserLongitude = order.DropoffLongitude,

                    Status = stageName,
                    DeliveryManName = order.Delivery != null
                        ? (($"{order.Delivery.FirstName} {order.Delivery.LastName}").Trim() is { Length: > 0 } full ? full : "مندوب برواح")
                        : string.Empty,
                    DeliveryManPhone = order.Delivery?.PhoneNumber ?? string.Empty,
                    Items = order.OrderItems.Select(i => new LiveTrackingItemDto
                    {
                        Name = i.Product?.Name ?? "منتج",
                        Quantity = i.Quantity,
                        Price = i.Price,
                        IsCancelled = i.MerchantStatus == OrderStatus.Cancelled
                    }).ToList(),
                    RefundAmount = order.PaymentMethod != PaymentMethod.Cash && cancelledSum > 0 ? (allCancelled ? cancelledSum + 30 : cancelledSum) : 0,
                    DeliveryManRating = 4.9,
                    TotalAmount = (decimal)order.TotalPrice,
                    PaymentStatus = order.PaymentMethod == PaymentMethod.Cash ? "الدفع نقداً عند الاستلام" : "مدفوع إلكترونياً"
                };

                return Ok(new { status = "success", data = liveTrackingDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
        #endregion

        #region Notification helpers
        private async Task SafeNotifyAsync(int userId, string title, string message)
        {
            try
            {
                await _notificationService.SendNotificationAsync(new SendNotification { UserId = userId, Title = title, Message = message });
            }
            catch { /* a failed notification must never fail the order */ }
        }

        private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double r = 6371;
            double dLat = (lat2 - lat1) * Math.PI / 180, dLon = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return 2 * r * Math.Asin(Math.Sqrt(a));
        }

        /// <summary>Tells each store it has a new order, and every active courier within 10 km of the pickup point (all couriers when the pickup has no location).</summary>
        private async Task NotifyStoresAndCouriersAsync(Order order)
        {
            try
            {
                var storeIds = await _context.OrderItems
                    .Where(i => i.OrderId == order.Id)
                    .Select(i => i.Product.UserId)
                    .Distinct().ToListAsync();
                foreach (var storeId in storeIds)
                    await SafeNotifyAsync(storeId, "طلب جديد 🛒", $"وصلك طلب جديد رقم #{order.Id}. راجعه واقبله من قائمة الطلبات.");

                // Food stores must accept first (their acceptance calls the couriers); an order
                // of other stores' items goes to the couriers right away.
                var full = await _context.Orders
                    .Include(o => o.OrderItems).ThenInclude(i => i.Product).ThenInclude(p => p.User)
                    .FirstAsync(o => o.Id == order.Id);
                if (OrderFlow.IsReadyForCouriers(full))
                {
                    var couriers = await _context.Users
                        .Where(u => u.UserType == UserType.Delivery && !u.IsFrozen)
                        .ToListAsync();
                    await OrderFlow.NotifyCouriersAsync(couriers, _notificationService, order);
                }
            }
            catch { /* best effort */ }
        }
        #endregion

        #region 6. My orders & service requests (طلباتي)
        [Authorize]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userAddress = (await _userManager.FindByIdAsync(userId.ToString()))?.Address ?? string.Empty;

                static string OrderStatusName(OrderStatus s) => s switch
                {
                    OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.OutForDelivery => "InProgress",
                    OrderStatus.Delivered => "Completed",
                    OrderStatus.Cancelled => "Cancelled",
                    _ => "Pending"
                };

                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems).ThenInclude(i => i.Product).ThenInclude(p => p.User)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var orderRows = orders.Select(o =>
                {
                    var live = o.OrderItems.Where(i => i.MerchantStatus != OrderStatus.Cancelled).ToList();
                    var first = live.FirstOrDefault() ?? o.OrderItems.FirstOrDefault();
                    var store = first?.Product?.User?.BusinessName;
                    return new
                    {
                        id = o.Id,
                        orderNumber = $"ORD-{o.Id}",
                        type = "order",
                        title = store != null ? $"طلب من {store}" : $"طلب #{o.Id}",
                        status = OrderStatusName(o.Status),
                        total = (decimal)o.TotalPrice,
                        items = o.OrderItems.Select(i =>
                            $"{i.Quantity} × {i.Product?.Name ?? "منتج"}" + (i.MerchantStatus == OrderStatus.Cancelled ? " (ملغي)" : "")).ToList(),
                        address = !string.IsNullOrWhiteSpace(o.DropoffAddress) ? o.DropoffAddress : userAddress,
                        createdAt = o.CreatedAt,
                        offersCount = 0,
                        hasCourier = o.DeliveryId != null,
                        courierRating = o.CourierRating
                    };
                }).ToList();

                var requests = await _context.ServiceRequests
                    .Where(r => r.UserId == userId)
                    .Include(r => r.SubCategory)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
                var requestIds = requests.Select(r => r.Id).ToList();
                var offerCounts = await _context.ServiceOffers
                    .Where(f => requestIds.Contains(f.ServiceRequestId) && !f.Withdrawn)
                    .GroupBy(f => f.ServiceRequestId)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Key, x => x.Count);
                var acceptedIds = requests.Where(r => r.AcceptedOfferId != null).Select(r => r.AcceptedOfferId!.Value).ToList();
                var acceptedPrices = await _context.ServiceOffers
                    .Where(f => acceptedIds.Contains(f.Id))
                    .ToDictionaryAsync(f => f.Id, f => f.Price);

                var requestRows = requests.Select(r => new
                {
                    id = r.Id,
                    orderNumber = $"SRV-{r.Id}",
                    type = "service",
                    title = r.SubCategory?.Name ?? "طلب خدمة",
                    status = r.Status switch
                    {
                        ServiceRequestStatus.Approved or ServiceRequestStatus.InProgress => "InProgress",
                        ServiceRequestStatus.Completed => "Completed",
                        ServiceRequestStatus.Cancelled => "Cancelled",
                        _ => "Pending"
                    },
                    total = r.AcceptedOfferId != null && acceptedPrices.TryGetValue(r.AcceptedOfferId.Value, out var p)
                        ? (decimal?)p
                        : (r.EstimatedPrice > 0 ? (decimal?)r.EstimatedPrice : null),
                    items = new List<string> { r.Description },
                    address = r.DeliveryAddress ?? r.PickupAddress ?? string.Empty,
                    createdAt = r.CreatedAt,
                    offersCount = offerCounts.TryGetValue(r.Id, out var c) ? c : 0
                }).ToList();

                var all = orderRows.Select(x => (x.createdAt, (object)x))
                    .Concat(requestRows.Select(x => (x.createdAt, (object)x)))
                    .OrderByDescending(x => x.Item1)
                    .Select(x => x.Item2)
                    .ToList();

                return Ok(new { status = "success", data = all });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
        #endregion

        #region 5. Admin Approval for Used Products (موافقة المشرف على المنتجات المستعملة)
        [Authorize(Roles = "Admin,SupervisorUsed")]
        [HttpPost("Admin/ApproveOrder/{orderId}")]
        public async Task<IActionResult> ApproveUsedOrder(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return NotFound(new { status = "error", message = "الطلب غير موجود" });

                order.Status = OrderStatus.Pending;
                order.ConditionStaus = "Approved - Ready for Delivery";

                await _context.SaveChangesAsync();

                await _notificationService.SendNotificationAsync(new SendNotification
                {
                    UserId = order.UserId,
                    Title = "تمت الموافقة على طلبك",
                    Message = $"تمت مراجعة طلبك المستعمل رقم #{order.Id} واعتماده بنجاح، وجاري إسناده لمندوب التوصيل."
                });
                await NotifyStoresAndCouriersAsync(order);

                return Ok(new
                {
                    status = "success",
                    message = "تمت الموافقة على الطلب بنجاح وتحويله إلى رادار المناديب.",
                    orderId = order.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
        #endregion
    }
}