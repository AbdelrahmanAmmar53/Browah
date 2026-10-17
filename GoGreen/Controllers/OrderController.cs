using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;
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

        public OrderController(ApplicationDbContext context, UserManager<User> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

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

                decimal deliveryFee = 30;
                decimal discount = 0;

                var summary = new OrderSummeryDto
                {
                    SubTotal = subTotal,
                    DeliveryFee = deliveryFee,
                    Discount = discount,
                    Total = subTotal + deliveryFee - discount
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

                decimal subTotal = 0;
                var orderItems = new List<OrderItem>();
                bool hasUsedProducts = false;

                // سنأخذ إحداثيات الاستلام (Pickup) من أول منتج أو تاجر في السلة كافتراضي
                double pickupLat = 30.0444; // قيمة افتراضية لحين توفر إحداثيات البائع
                double pickupLon = 31.2357;

                foreach (var cartItem in model.Items)
                {
                    var product = await _context.Products
                        .Include(p => p.User) // جلب بيانات التاجر/المتجر المالك للمنتج
                        .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

                    if (product == null || product.Stock < cartItem.Quantity)
                        return BadRequest(new { status = "error", message = "المنتج غير موجود أو الكمية غير متوفرة" });

                    // إذا كان التاجر يمتلك إحداثيات مسجلة في حسابه، نأخذها كموقع استلام الطلب
                    if (product.User != null)
                    {
                        // (ملاحظة: تأكد من إضافة Latitude و Longitude لجدول User/Merchant لو أردت، أو استخدم قيم افتراضية)
                        pickupLat = product.User.Latitude; 
                        // pickupLon = product.User.Longitude;
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
                        Price = product.Price
                    });

                    product.Stock -= cartItem.Quantity;
                }

                decimal deliveryFee = 30;
                decimal finalTotal = subTotal + deliveryFee;

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

                    OrderItems = orderItems
                };

                if (hasUsedProducts)
                {
                    order.Status = OrderStatus.Pending;
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
                    new() { Title = "في الطريق إليك", Subtitle = "سريع جداً", IsCompleted = order.Status >= OrderStatus.Shipped, IsCurrent = order.Status == OrderStatus.Shipped },
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
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return NotFound(new { status = "error", message = "الطلب غير موجود" });

                var liveTrackingDto = new LiveTrackingDto
                {
                    OrderId = order.Id,
                    RemainingTime = "12 - 15 دقيقة",

                    // --- التعديل الهام هنا: استخدام الإحداثيات الفعلية المخزنة للطلب بدل الأرقام الثابتة ---
                    DeliveryLatitude = order.PickupLatitude,   // موقع المطعم/الاستلام كبداية تحرك الكابتن
                    DeliveryLongitude = order.PickupLongitude,
                    UserLatitude = order.DropoffLatitude,       // موقع العميل الحقيقي
                    UserLongitude = order.DropoffLongitude,

                    DeliveryManName = order.Delivery != null ? order.Delivery.UserName ?? "مندوب برواح" : "في انتظار قبول المندوب",
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