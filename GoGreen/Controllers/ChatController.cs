using Berwah.BL.Service;
using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Hubs;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Berwah.Controllers
{
    /// <summary>
    /// Chat between two accounts once there is something to talk about:
    /// a service request whose offer was accepted (customer ↔ provider), or an order that went through
    /// (buyer ↔ seller; a store that must accept first only after it accepted).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private const int KindService = 1;
        private const int KindOrder = 2;
        private const int KindDelivery = 3;
        /// <summary>Product supervisor talking to the ad's owner while the ad waits for a decision.</summary>
        private const int KindAdReview = 4;

        private readonly ApplicationDbContext _db;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly INotificationService _notifications;
        private readonly IImageService _imageService;

        public ChatController(ApplicationDbContext db, IHubContext<NotificationHub> hub, INotificationService notifications, IImageService imageService)
        {
            _imageService = imageService;
            _db = db;
            _hub = hub;
            _notifications = notifications;
        }

        private int Me => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static string NameOf(User u) =>
            !string.IsNullOrWhiteSpace(u.BusinessName) ? u.BusinessName!.Trim() : $"{u.FirstName} {u.LastName}".Trim();

        /// <summary>May this seller and the buyer talk about this order yet? Needs the items' Product.User loaded.</summary>
        private static bool OrderChatOpen(Order order, int sellerId)
        {
            if (order.Status == OrderStatus.PendingAdminApproval || order.Status == OrderStatus.Cancelled) return false;
            var items = order.OrderItems.Where(i => i.Product != null && i.Product.UserId == sellerId
                                                    && i.MerchantStatus != OrderStatus.Cancelled).ToList();
            if (items.Count == 0) return false;
            var seller = items[0].Product!.User;
            // A person selling their own item, or a store that needs no acceptance: open once the purchase went through.
            if (seller == null || string.IsNullOrWhiteSpace(seller.BusinessType) || !OrderFlow.IsFoodStore(seller.BusinessType)) return true;
            // A restaurant / grocery / home kitchen: after it accepted its part.
            return items.Any(i => (i.MerchantStatus ?? OrderStatus.Pending) != OrderStatus.Pending);
        }

        private async Task<Conversation> EnsureAsync(int kind, int refId, int a, int b)
        {
            var x = Math.Min(a, b);
            var y = Math.Max(a, b);
            var c = await _db.Conversations.FirstOrDefaultAsync(v => v.Kind == kind && v.RefId == refId && v.UserAId == x && v.UserBId == y);
            if (c != null) return c;
            c = new Conversation { Kind = kind, RefId = refId, UserAId = x, UserBId = y };
            _db.Conversations.Add(c);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Both sides opened it at the same moment: the other one won.
                _db.Entry(c).State = EntityState.Detached;
                c = await _db.Conversations.FirstAsync(v => v.Kind == kind && v.RefId == refId && v.UserAId == x && v.UserBId == y);
            }
            return c;
        }

        /// <summary>Creates the chats the caller is allowed to have but that nobody opened yet.</summary>
        private async Task SyncAsync(int me)
        {
            // Service requests whose offer was accepted (I asked, or I am the chosen provider).
            var accepted = await _db.ServiceRequests.AsNoTracking()
                .Where(r => r.AcceptedOfferId != null)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    ProviderId = _db.ServiceOffers.Where(o => o.Id == r.AcceptedOfferId).Select(o => (int?)o.ProviderId).FirstOrDefault()
                })
                .Where(r => r.UserId == me || r.ProviderId == me)
                .ToListAsync();
            foreach (var r in accepted)
                if (r.ProviderId != null) await EnsureAsync(KindService, r.Id, r.UserId, r.ProviderId.Value);

            // Orders I bought or sold in (latest first, so a long history stays cheap).
            var orders = await _db.Orders.AsNoTracking()
                .Include(o => o.OrderItems).ThenInclude(i => i.Product).ThenInclude(p => p.User)
                .Where(o => o.UserId == me || o.OrderItems.Any(i => i.Product.UserId == me))
                .OrderByDescending(o => o.Id)
                .Take(100)
                .ToListAsync();
            foreach (var o in orders)
            {
                var sellers = o.OrderItems.Where(i => i.Product != null).Select(i => i.Product!.UserId).Distinct();
                foreach (var s in sellers)
                {
                    if (s == o.UserId) continue;
                    if (o.UserId != me && s != me) continue;
                    if (OrderChatOpen(o, s)) await EnsureAsync(KindOrder, o.Id, o.UserId, s);
                }
            }
        }

        /// <summary>Chats between a courier and the people of the order he took (the customer and each store).</summary>
        private async Task SyncDeliveryAsync(int me)
        {
            var orders = await _db.Orders.AsNoTracking()
                .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .Where(o => o.DeliveryId != null && o.Status != OrderStatus.Cancelled
                            && (o.DeliveryId == me || o.UserId == me || o.OrderItems.Any(i => i.Product.UserId == me)))
                .OrderByDescending(o => o.Id)
                .Take(50)
                .ToListAsync();
            foreach (var o in orders)
            {
                var courier = o.DeliveryId!.Value;
                var others = new HashSet<int> { o.UserId };
                foreach (var i in o.OrderItems.Where(i => i.Product != null && i.MerchantStatus != OrderStatus.Cancelled))
                    others.Add(i.Product!.UserId);
                others.Remove(courier);
                foreach (var other in others)
                {
                    if (courier != me && other != me) continue;
                    await EnsureAsync(KindDelivery, o.Id, courier, other);
                }
            }
        }

        /// <summary>My chats, the latest first.</summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var me = Me;
                await SyncAsync(me);
                await SyncDeliveryAsync(me);

                var convs = await _db.Conversations.AsNoTracking()
                    .Where(c => c.UserAId == me || c.UserBId == me)
                    .OrderByDescending(c => c.LastMessageAt)
                    .ToListAsync();

                var otherIds = convs.Select(c => c.UserAId == me ? c.UserBId : c.UserAId).Distinct().ToList();
                var users = await _db.Users.AsNoTracking().Where(u => otherIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);
                var ids = convs.Select(c => c.Id).ToList();
                var lastIds = await _db.ChatMessages.AsNoTracking()
                    .Where(m => ids.Contains(m.ConversationId))
                    .GroupBy(m => m.ConversationId)
                    .Select(g => g.Max(m => m.Id))
                    .ToListAsync();
                var lastMessages = await _db.ChatMessages.AsNoTracking()
                    .Where(m => lastIds.Contains(m.Id))
                    .ToListAsync();
                var lastBy = lastMessages.ToDictionary(m => m.ConversationId);
                var unread = await _db.ChatMessages.AsNoTracking()
                    .Where(m => ids.Contains(m.ConversationId) && m.SenderId != me && m.ReadAt == null)
                    .GroupBy(m => m.ConversationId)
                    .Select(g => new { Id = g.Key, N = g.Count() })
                    .ToDictionaryAsync(x => x.Id, x => x.N);

                var closedBy = await ClosedStateAsync(convs);

                var data = convs.Select(c =>
                {
                    closedBy.TryGetValue(c.Id, out var closedState);
                    var otherId = c.UserAId == me ? c.UserBId : c.UserAId;
                    users.TryGetValue(otherId, out var other);
                    lastBy.TryGetValue(c.Id, out var last);
                    return new
                    {
                        id = c.Id,
                        kind = c.Kind,
                        refId = c.RefId,
                        otherUserId = otherId,
                        otherName = other == null ? string.Empty : NameOf(other),
                        otherImage = other?.ProfileImageUrl,
                        lastMessage = last == null ? string.Empty : (string.IsNullOrEmpty(last.Text) && last.ImageUrl != null ? "📷 صورة" : last.Text),
                        lastMessageAt = last?.CreatedAt ?? c.CreatedAt,
                        unread = unread.TryGetValue(c.Id, out var n) ? n : 0,
                        closed = closedState.Closed,
                        closedAt = closedState.At
                    };
                }).ToList();

                return Ok(new { status = "success", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        /// <summary>Who of an order's other side I can chat with right now (for the buyer: the sellers).</summary>
        [HttpGet("order/{orderId}/parties")]
        public async Task<IActionResult> OrderParties(int orderId)
        {
            var me = Me;
            var order = await _db.Orders.AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.OrderItems).ThenInclude(i => i.Product).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound(new { status = "error", message = "الطلب غير موجود" });

            var sellerIds = order.OrderItems.Where(i => i.Product != null).Select(i => i.Product!.UserId).Distinct().ToList();

            // The courier who took the order talks to the customer and to each store.
            if (order.DeliveryId == me)
            {
                var people = new List<User>();
                if (order.User != null) people.Add(order.User);
                people.AddRange(order.OrderItems.Where(i => i.Product?.User != null && i.Product.UserId != order.UserId)
                    .Select(i => i.Product!.User!).DistinctBy(u => u.Id));
                var courierData = people.Select(u => new
                {
                    userId = u.Id, name = NameOf(u), image = u.ProfileImageUrl, canChat = true, kind = KindDelivery
                }).ToList();
                return Ok(new { status = "success", data = courierData });
            }

            if (order.UserId != me && !sellerIds.Contains(me))
                return NotFound(new { status = "error", message = "الطلب غير موجود" });

            IEnumerable<User> others = order.UserId == me
                ? order.OrderItems.Where(i => i.Product?.User != null && i.Product.UserId != me).Select(i => i.Product!.User!).DistinctBy(u => u.Id)
                : new[] { order.User! };

            var data = others.Select(u => new
            {
                userId = u.Id,
                name = NameOf(u),
                image = u.ProfileImageUrl,
                canChat = order.UserId == me ? OrderChatOpen(order, u.Id) : OrderChatOpen(order, me),
                kind = KindOrder
            }).ToList();

            // Once a courier took the order, the buyer and the stores can reach him too.
            var dataList = data.Cast<object>().ToList();
            if (order.DeliveryId != null)
            {
                var courier = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == order.DeliveryId);
                if (courier != null)
                    dataList.Add(new { userId = courier.Id, name = NameOf(courier), image = courier.ProfileImageUrl, canChat = true, kind = KindDelivery });
            }
            return Ok(new { status = "success", data = dataList });
        }

        public class OpenChatDto
        {
            public int Kind { get; set; }
            public int RefId { get; set; }
            public int OtherUserId { get; set; }
        }

        /// <summary>Opens (or finds) the chat about a request / an order with one other person.</summary>
        [HttpPost("open")]
        public async Task<IActionResult> Open([FromBody] OpenChatDto dto)
        {
            try
            {
                var me = Me;
                var peer = dto.OtherUserId;
                if (dto.OtherUserId == me)
                    return BadRequest(new { status = "error", message = "لا يمكن بدء محادثة مع نفسك" });

                if (dto.Kind == KindService)
                {
                    var req = await _db.ServiceRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == dto.RefId);
                    var providerId = req?.AcceptedOfferId == null ? null
                        : await _db.ServiceOffers.Where(o => o.Id == req.AcceptedOfferId).Select(o => (int?)o.ProviderId).FirstOrDefaultAsync();
                    if (req == null || providerId == null)
                        return BadRequest(new { status = "error", message = "المحادثة تُفتح بعد قبول العرض" });
                    var pair = new[] { req.UserId, providerId.Value };
                    if (!pair.Contains(me))
                        return NotFound(new { status = "error", message = "المحادثة غير متاحة" });
                    peer = me == req.UserId ? providerId.Value : req.UserId;
                }
                else if (dto.Kind == KindOrder)
                {
                    var order = await _db.Orders.AsNoTracking()
                        .Include(o => o.OrderItems).ThenInclude(i => i.Product).ThenInclude(p => p.User)
                        .FirstOrDefaultAsync(o => o.Id == dto.RefId);
                    if (order == null) return NotFound(new { status = "error", message = "الطلب غير موجود" });
                    var buyer = order.UserId;
                    var seller = buyer == me ? dto.OtherUserId : me;
                    var other = buyer == me ? dto.OtherUserId : buyer;
                    peer = other;
                    if (!OrderChatOpen(order, seller))
                        return BadRequest(new { status = "error", message = "المحادثة تُفتح بعد إتمام الشراء وقبول المتجر للطلب" });
                }
                else if (dto.Kind == KindDelivery)
                {
                    var order = await _db.Orders.AsNoTracking()
                        .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(o => o.Id == dto.RefId);
                    if (order == null) return NotFound(new { status = "error", message = "الطلب غير موجود" });
                    if (order.DeliveryId == null)
                        return BadRequest(new { status = "error", message = "المحادثة تُفتح بعد قبول مندوب التوصيل" });
                    var courier = order.DeliveryId.Value;
                    var sellers = order.OrderItems.Where(i => i.Product != null).Select(i => i.Product!.UserId).ToList();
                    if (me == courier)
                    {
                        if (dto.OtherUserId != order.UserId && !sellers.Contains(dto.OtherUserId))
                            return NotFound(new { status = "error", message = "المحادثة غير متاحة" });
                        peer = dto.OtherUserId;
                    }
                    else if (me == order.UserId || sellers.Contains(me))
                    {
                        peer = courier;
                    }
                    else
                    {
                        return NotFound(new { status = "error", message = "المحادثة غير متاحة" });
                    }
                }
                else if (dto.Kind == KindAdReview)
                {
                    var isSupervisor = User.IsInRole("SupervisorForAd") || User.IsInRole("SupervisorUsed") || User.IsInRole("Admin") || User.IsInRole("BossOfSupervisor");
                    if (!isSupervisor)
                        return StatusCode(403, new { status = "error", message = "هذه المحادثة للمشرفين فقط" });
                    var ad = await _db.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == dto.RefId);
                    if (ad == null) return NotFound(new { status = "error", message = "الإعلان غير موجود" });
                    peer = ad.UserId;
                    if (peer == me)
                        return BadRequest(new { status = "error", message = "لا يمكن بدء محادثة مع نفسك" });
                }
                else
                {
                    return BadRequest(new { status = "error", message = "نوع المحادثة غير صحيح" });
                }

                var c = await EnsureAsync(dto.Kind, dto.RefId, me, peer);
                return Ok(new { status = "success", data = new { id = c.Id, closed = await IsClosedAsync(c) } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        /// <summary>
        /// Which of these chats are over: the service was completed or cancelled, or the order was
        /// delivered or cancelled. Worked out from the thing itself, so nothing is stored.
        /// </summary>
        private async Task<Dictionary<int, (bool Closed, DateTime? At)>> ClosedStateAsync(IEnumerable<Conversation> convs)
        {
            var result = new Dictionary<int, (bool, DateTime?)>();
            var list = convs.ToList();

            var serviceIds = list.Where(c => c.Kind == KindService).Select(c => c.RefId).Distinct().ToList();
            var services = serviceIds.Count == 0
                ? new Dictionary<int, (ServiceRequestStatus Status, DateTime? At)>()
                : (await _db.ServiceRequests.AsNoTracking().Where(r => serviceIds.Contains(r.Id))
                    .Select(r => new { r.Id, r.Status, r.CompletedAt }).ToListAsync())
                    .ToDictionary(r => r.Id, r => (r.Status, r.CompletedAt));

            var adIds = list.Where(c => c.Kind == KindAdReview).Select(c => c.RefId).Distinct().ToList();
            var ads = adIds.Count == 0
                ? new Dictionary<int, (AdStatus Status, DateTime? At)>()
                : (await _db.Advertisements.AsNoTracking().Where(a => adIds.Contains(a.Id))
                    .Select(a => new { a.Id, a.Status, a.ReviewedAt }).ToListAsync())
                    .ToDictionary(a => a.Id, a => (a.Status, a.ReviewedAt));

            var orderIds = list.Where(c => c.Kind == KindOrder || c.Kind == KindDelivery).Select(c => c.RefId).Distinct().ToList();
            var orders = orderIds.Count == 0
                ? new Dictionary<int, (OrderStatus Status, DateTime? At)>()
                : (await _db.Orders.AsNoTracking().Where(o => orderIds.Contains(o.Id))
                    .Select(o => new { o.Id, o.Status, o.DeliveredAt }).ToListAsync())
                    .ToDictionary(o => o.Id, o => (o.Status, o.DeliveredAt));

            foreach (var c in list)
            {
                if (c.Kind == KindService)
                {
                    if (!services.TryGetValue(c.RefId, out var s)) { result[c.Id] = (true, null); continue; }
                    result[c.Id] = (s.Status == ServiceRequestStatus.Completed || s.Status == ServiceRequestStatus.Cancelled, s.Item2);
                }
                else if (c.Kind == KindAdReview)
                {
                    // Open only while the ad still waits for the supervisor's decision.
                    if (!ads.TryGetValue(c.RefId, out var a)) { result[c.Id] = (true, null); continue; }
                    result[c.Id] = (a.Status != AdStatus.Pending, a.Status != AdStatus.Pending ? a.Item2 : null);
                }
                else
                {
                    if (!orders.TryGetValue(c.RefId, out var o)) { result[c.Id] = (true, null); continue; }
                    result[c.Id] = (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Cancelled, o.Item2);
                }
            }
            return result;
        }

        private const string ClosedMessage = "انتهت العملية، المحادثة مقفولة";

        private async Task<bool> IsClosedAsync(Conversation c) => (await ClosedStateAsync(new[] { c }))[c.Id].Closed;

        private async Task<Conversation?> MineAsync(int id)
        {
            var me = Me;
            return await _db.Conversations.FirstOrDefaultAsync(c => c.Id == id && (c.UserAId == me || c.UserBId == me));
        }

        /// <summary>The messages of a chat, oldest first; pass afterId to get only the newer ones.</summary>
        [HttpGet("{id}/messages")]
        public async Task<IActionResult> Messages(int id, [FromQuery] long afterId = 0)
        {
            var c = await MineAsync(id);
            if (c == null) return NotFound(new { status = "error", message = "المحادثة غير موجودة" });

            var me = Me;
            var list = await _db.ChatMessages.AsNoTracking()
                .Where(m => m.ConversationId == id && m.Id > afterId)
                .OrderByDescending(m => m.Id).Take(200).OrderBy(m => m.Id)
                .Select(m => new { id = m.Id, senderId = m.SenderId, text = m.Text, imageUrl = m.ImageUrl, createdAt = m.CreatedAt, mine = m.SenderId == me })
                .ToListAsync();

            // What the other side sent is read now.
            var unread = await _db.ChatMessages.Where(m => m.ConversationId == id && m.SenderId != me && m.ReadAt == null).ToListAsync();
            if (unread.Count > 0)
            {
                foreach (var m in unread) m.ReadAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            // How far the other side has read what I sent: every message of mine up to this id was seen.
            var readUpTo = await _db.ChatMessages.AsNoTracking()
                .Where(m => m.ConversationId == id && m.SenderId == me && m.ReadAt != null)
                .Select(m => (long?)m.Id).MaxAsync() ?? 0;

            return Ok(new { status = "success", data = list, closed = await IsClosedAsync(c), readUpTo });
        }

        public class SendChatDto
        {
            public string? Text { get; set; }
        }

        [HttpPost("{id}/messages")]
        public async Task<IActionResult> Send(int id, [FromBody] SendChatDto dto)
        {
            try
            {
                var c = await MineAsync(id);
                if (c == null) return NotFound(new { status = "error", message = "المحادثة غير موجودة" });
                if (await IsClosedAsync(c)) return BadRequest(new { status = "error", message = ClosedMessage, closed = true });

                var text = dto.Text?.Trim();
                if (string.IsNullOrEmpty(text)) return BadRequest(new { status = "error", message = "الرسالة فارغة" });
                if (text.Length > 1000) text = text[..1000];

                var me = Me;
                var msg = new ChatMessage { ConversationId = c.Id, SenderId = me, Text = text, CreatedAt = DateTime.UtcNow };
                _db.ChatMessages.Add(msg);
                c.LastMessageAt = msg.CreatedAt;
                await _db.SaveChangesAsync();

                var otherId = c.UserAId == me ? c.UserBId : c.UserAId;
                var sender = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == me);
                var payload = new { id = msg.Id, conversationId = c.Id, senderId = me, senderName = NameOf(sender), text = msg.Text, createdAt = msg.CreatedAt };
                try { await _hub.Clients.User(otherId.ToString()).SendAsync("ReceiveChatMessage", payload); } catch { }
                try
                {
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = otherId,
                        Title = NameOf(sender),
                        Message = text.Length > 120 ? text[..120] + "…" : text,
                        Type = "chat",
                        RefId = c.Id
                    });
                }
                catch { /* best effort */ }

                return Ok(new { status = "success", data = new { id = msg.Id, createdAt = msg.CreatedAt } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }

        public class SendChatImageDto
        {
            public IFormFile? Image { get; set; }
            public string? Text { get; set; }
        }

        /// <summary>Sends a photo (with an optional caption) into the chat.</summary>
        [HttpPost("{id}/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendImage(int id, [FromForm] SendChatImageDto dto)
        {
            try
            {
                var c = await MineAsync(id);
                if (c == null) return NotFound(new { status = "error", message = "المحادثة غير موجودة" });
                if (await IsClosedAsync(c)) return BadRequest(new { status = "error", message = ClosedMessage, closed = true });

                var file = dto.Image;
                if (file == null || file.Length == 0) return BadRequest(new { status = "error", message = "الصورة مطلوبة" });
                if (file.Length > 8 * 1024 * 1024) return BadRequest(new { status = "error", message = "الصورة كبيرة جداً (الحد 8 ميجا)" });
                if (file.ContentType == null || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { status = "error", message = "الملف لازم يكون صورة" });

                var up = await _imageService.UploadImageAsync(file);
                var url = up?.SecureUrl?.ToString();
                if (string.IsNullOrEmpty(url)) return StatusCode(500, new { status = "error", message = "تعذر رفع الصورة، حاول مرة أخرى" });

                var caption = dto.Text?.Trim() ?? string.Empty;
                if (caption.Length > 1000) caption = caption[..1000];

                var me = Me;
                var msg = new ChatMessage { ConversationId = c.Id, SenderId = me, Text = caption, ImageUrl = url, CreatedAt = DateTime.UtcNow };
                _db.ChatMessages.Add(msg);
                c.LastMessageAt = msg.CreatedAt;
                await _db.SaveChangesAsync();

                var otherId = c.UserAId == me ? c.UserBId : c.UserAId;
                var sender = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == me);
                var payload = new { id = msg.Id, conversationId = c.Id, senderId = me, senderName = NameOf(sender), text = msg.Text, imageUrl = url, createdAt = msg.CreatedAt };
                try { await _hub.Clients.User(otherId.ToString()).SendAsync("ReceiveChatMessage", payload); } catch { }
                try
                {
                    await _notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = otherId,
                        Title = NameOf(sender),
                        Message = caption.Length == 0 ? "📷 صورة" : "📷 " + (caption.Length > 110 ? caption[..110] + "…" : caption),
                        Type = "chat",
                        RefId = c.Id
                    });
                }
                catch { /* best effort */ }

                return Ok(new { status = "success", data = new { id = msg.Id, createdAt = msg.CreatedAt, imageUrl = url } });
            }
            catch (Exception)
            {
                return StatusCode(500, new { status = "error", message = "حدث خطأ غير متوقع، حاول مرة أخرى" });
            }
        }
    }
}
