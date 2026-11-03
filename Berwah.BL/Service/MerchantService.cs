using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.BL.Service
{
    public class MerchantService : IMerchantService
    {
        private readonly IMerchantRepository _merchantRepository;
        private readonly INotificationService _notificationService;
        public MerchantService(IMerchantRepository merchantRepository, INotificationService notificationService)
        {
            _merchantRepository = merchantRepository;
            _notificationService = notificationService;
        }

        public async Task<MerchantDashboardResponseDto> GetDashboardDataAsync(int merchantId, string? filter)
        {
            var merchant = await _merchantRepository.GetMerchantByIdAsync(merchantId);
            if (merchant == null) throw new Exception("التاجر غير موجود");

            var allOrders = await _merchantRepository.GetMerchantOrdersAsync(merchantId);

            // An order can hold items of several stores: each store sees its own part and
            // its own progress (StatusFor), not the other stores'.
            int totalActive = allOrders.Count(o => StatusFor(o, merchantId) != OrderStatus.Delivered
                                                   && StatusFor(o, merchantId) != OrderStatus.Cancelled);
            int cookingCount = allOrders.Count(o => StatusFor(o, merchantId) == OrderStatus.Processing);
            int readyCount = allOrders.Count(o =>
            {
                var st = StatusFor(o, merchantId);
                return st == OrderStatus.Shipped || (o.DeliveryId != null && st == OrderStatus.Processing);
            });

            decimal todaySales = allOrders
                .Where(o => o.CreatedAt.Date == DateTime.UtcNow.Date && o.Status == OrderStatus.Delivered)
                .Sum(o => TotalFor(o, merchantId));

            var filteredOrders = filter?.ToLower() switch
            {
                "new" => allOrders.Where(o => StatusFor(o, merchantId) == OrderStatus.Pending).ToList(),
                "cooking" => allOrders.Where(o => StatusFor(o, merchantId) == OrderStatus.Processing).ToList(),
                "ready" => allOrders.Where(o => o.DeliveryId != null).ToList(),
                _ => allOrders
            };

            var orderDtos = filteredOrders.Select(o => new MerchantOrderDto
            {
                OrderId = o.Id,
                OrderNumber = $"#ORD-{o.Id}",
                CustomerName = $"{o.User?.FirstName} {o.User?.LastName}".Trim(),
                CustomerAddress = !string.IsNullOrWhiteSpace(o.DropoffAddress) ? o.DropoffAddress! : (o.User?.Address ?? "العنوان المحدد"),
                TotalPrice = TotalFor(o, merchantId),
                Status = StatusFor(o, merchantId).ToString(),
                StatusBadgeText = StatusFor(o, merchantId) == OrderStatus.Pending ? "طلب جديد" : "قيد التجهيز",
                PaymentMethod = o.PaymentMethod == PaymentMethod.Cash ? "نقداً (كاش)" : "دفع إلكتروني",
                HasCaptain = o.DeliveryId != null,
                CaptainName = o.Delivery == null ? null : $"{o.Delivery.FirstName} {o.Delivery.LastName}".Trim(),
                Items = OwnItems(o, merchantId).Select(oi => new MerchantOrderItemDto
                {
                    ProductName = oi.Product?.Name ?? "منتج",
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            }).ToList();

            return new MerchantDashboardResponseDto
            {
                StoreName = merchant.BusinessName ?? "متجر الشريك",
                BranchName = merchant.Address ?? "الفرع الرئيسي",
                IsOpenForOrders = !merchant.IsFrozen && !merchant.IsPaused,
                TotalActiveOrders = totalActive,
                CookingOrAssemblyOrders = cookingCount,
                ReadyForDeliveryOrders = readyCount,
                TodaySales = todaySales,
                Orders = orderDtos
            };
        }

        // ---- Orders that hold items of several stores ----

        /// <summary>The items of <paramref name="o"/> that belong to this merchant.</summary>
        private static List<OrderItem> OwnItems(Order o, int merchantId) =>
            o.OrderItems.Where(oi => oi.Product != null && oi.Product.UserId == merchantId).ToList();

        /// <summary>
        /// What the merchant earns/handles in the order: the whole total when every item is
        /// his, otherwise just his items.
        /// </summary>
        private static decimal TotalFor(Order o, int merchantId)
        {
            var own = OwnItems(o, merchantId);
            if (own.Count == 0) return (decimal)o.TotalPrice;
            var live = own.Where(i => i.MerchantStatus != OrderStatus.Cancelled).ToList();
            // Everything in the order is his and nothing was cancelled: the whole total.
            if (live.Count == own.Count && own.Count == o.OrderItems.Count) return (decimal)o.TotalPrice;
            return live.Sum(i => i.Price * i.Quantity);
        }

        /// <summary>The order's progress as this merchant sees it (his own items only).</summary>
        private static OrderStatus StatusFor(Order o, int merchantId)
        {
            if (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Cancelled
                || o.Status == OrderStatus.PendingAdminApproval) return o.Status;

            var own = OwnItems(o, merchantId);
            if (own.Count == 0) return o.Status;
            if (own.All(i => i.MerchantStatus == OrderStatus.Cancelled)) return OrderStatus.Cancelled;
            if (o.Status == OrderStatus.OutForDelivery) return OrderStatus.Shipped;
            own = own.Where(i => i.MerchantStatus != OrderStatus.Cancelled).ToList();

            var states = own.Select(i => i.MerchantStatus ?? o.Status).ToList();
            if (states.All(s => s == OrderStatus.Shipped)) return OrderStatus.Shipped;
            if (states.Any(s => s == OrderStatus.Processing || s == OrderStatus.Shipped)) return OrderStatus.Processing;
            return states.First();
        }

        /// <summary>
        /// The whole order is ready for the courier only when every store's items are; as soon
        /// as one store starts, the order is being prepared.
        /// </summary>
        private static void RecalculateOrderStatus(Order order)
        {
            // Once the courier has the goods (or the order ended) the stores no longer move it.
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled
                || order.Status == OrderStatus.OutForDelivery) return;

            // A store that rejected its part is out of the picture.
            var states = order.OrderItems
                .Where(i => i.MerchantStatus != OrderStatus.Cancelled)
                .Select(i => i.MerchantStatus ?? order.Status).ToList();
            if (states.Count == 0) return;

            if (states.All(s => s == OrderStatus.Shipped))
            {
                order.Status = OrderStatus.Shipped;
                order.ConditionStaus = "جاهز لتسليم الوجبات للمندوب";
            }
            else if (states.Any(s => s == OrderStatus.Processing || s == OrderStatus.Shipped))
            {
                order.Status = OrderStatus.Processing;
                order.ConditionStaus = states.Any(s => s == OrderStatus.Shipped)
                    ? "بعض المحلات جهزت طلبك والباقي قيد التجهيز"
                    : "قيد الطهي والتجهيز";
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, int merchantId, UpdateMerchantOrderStatusDto model)
        {
            var order = await _merchantRepository.GetMerchantOrderWithDetailsAsync(orderId, merchantId);
            if (order == null) return false;

            var ownItems = OwnItems(order, merchantId);
            bool wasReadyForCouriers = OrderFlow.IsReadyForCouriers(order);
            if (model.NewStatus == "Cooking")
            {
                foreach (var item in ownItems) item.MerchantStatus = OrderStatus.Processing;
                RecalculateOrderStatus(order);
            }
            else if (model.NewStatus == "Ready")
            {
                foreach (var item in ownItems) item.MerchantStatus = OrderStatus.Shipped;
                RecalculateOrderStatus(order);
            }
            else if (model.NewStatus == "Cancelled")
            {
                order.Status = OrderStatus.Cancelled;
                foreach (var item in order.OrderItems) item.MerchantStatus = OrderStatus.Cancelled;
                order.ConditionStaus = "تم إلغاء الطلب من قبل المتجر";
            }

            await _merchantRepository.UpdateOrderAsync(order);

            // The last food store accepted: the order now goes to the couriers.
            if (!wasReadyForCouriers && order.DeliveryId == null && order.Status != OrderStatus.Cancelled
                && OrderFlow.IsReadyForCouriers(order))
            {
                var couriers = await _merchantRepository.GetActiveCouriersAsync();
                await OrderFlow.NotifyCouriersAsync(couriers, _notificationService, order);
            }

            if (order.UserId > 0)
            {
                var merchantName = order.OrderItems.FirstOrDefault()?.Product?.User?.UserName ?? "المتجر";
                await _notificationService.SendNotificationAsync(new SendNotification
                {
                    UserId = order.UserId,
                    Title = "تحديث طلبك 🍔",
                    Message = $"متجر {merchantName} قام بتحديث حالة طلبك إلى: {order.ConditionStaus}"
                });
            }

            return true;
        }

        public async Task<bool> ToggleStoreStatusAsync(int merchantId, bool isOpen)
        {
            var merchant = await _merchantRepository.GetMerchantByIdAsync(merchantId);
            if (merchant == null) return false;

            merchant.IsFrozen = !isOpen;
            if (isOpen) merchant.PausedUntil = null;
            await _merchantRepository.UpdateMerchantAsync(merchant);
            return true;
        }
        // True once this merchant has accepted (or started) its part of the order.
        private static bool IsAccepted(Order order, int merchantId)
        {
            return OwnItems(order, merchantId)
                .Where(i => i.MerchantStatus != OrderStatus.Cancelled)
                .Select(i => i.MerchantStatus ?? order.Status)
                .Any(st => st == OrderStatus.Processing || st == OrderStatus.Shipped);
        }

        public async Task<RejectOrderDetailsDto?> GetRejectOrderDetailsAsync(int orderId, int merchantId)
        {
            var order = await _merchantRepository.GetMerchantOrderWithDetailsAsync(orderId, merchantId);
            if (order == null) return null;
            return new RejectOrderDetailsDto
            {
                orderid = order.Id,
                OrderNumber = $"#ORD-{order.Id}",
                CustomerName = $"{order.User?.FirstName} {order.User?.LastName}".Trim(),
                CustomerAddress = !string.IsNullOrWhiteSpace(order.DropoffAddress) ? order.DropoffAddress! : (order.User?.Address ?? "العنوان المحدد"),
                TotalPrice = TotalFor(order, merchantId),
                Items = OwnItems(order, merchantId).Select(oi => new MerchantOrderItemDto
                {
                    ProductName = oi.Product?.Name ?? "منتج",
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList(),
                PenaltyAmount = IsAccepted(order, merchantId) ? 35.00m : 0m,
                AcceptanceRateImpact = IsAccepted(order, merchantId) ? "-2.5%" : "",
                AppearancePenaltyDuration = IsAccepted(order, merchantId) ? "4 ساعات" : ""
            };
        }
        public async Task<bool> ConfirmOrderRejectionAsync(int orderId, int merchantId, ConfirmRejectOrderDto model)
        {
            var order = await _merchantRepository.GetMerchantOrderWithDetailsAsync(orderId, merchantId);
            if (order == null) return false;


            var own = OwnItems(order, merchantId).Where(i => i.MerchantStatus != OrderStatus.Cancelled).ToList();
            if (own.Count == 0) return false;
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled
                || order.Status == OrderStatus.OutForDelivery) return false;

            // Before accepting: no penalty at all. After accepting/starting: the agreed penalty applies.
            bool wasAccepted = IsAccepted(order, merchantId);
            decimal penaltyFee = wasAccepted ? 35.00m : 0m;

            var rejecting = await _merchantRepository.GetMerchantByIdAsync(merchantId);
            var storeName = rejecting?.BusinessName ?? "المتجر";
            bool othersRemain = order.OrderItems.Any(i => i.MerchantStatus != OrderStatus.Cancelled && !own.Contains(i));

            // Only this store's part is cancelled: stock goes back and its price comes off the total
            // (the delivery fee stays in the total).
            foreach (var item in own)
            {
                item.MerchantStatus = OrderStatus.Cancelled;
                if (item.Product != null) item.Product.Stock += item.Quantity;
            }
            decimal paidBefore = (decimal)order.TotalPrice;
            decimal ownSum = own.Sum(i => i.Price * i.Quantity);
            order.TotalPrice -= (float)ownSum;
            if (order.TotalPrice < 0) order.TotalPrice = 0;

            if (!othersRemain)
            {
                order.Status = OrderStatus.Cancelled;
                order.ConditionStaus = $"تم إلغاء الطلب من قبل التاجر (السبب: {model.RejectionReason})";
            }
            else
            {
                order.ConditionStaus = $"تم إلغاء أصناف {storeName} والباقي مستمر";
                RecalculateOrderStatus(order);
            }
            await _merchantRepository.UpdateOrderAsync(order);

            if (penaltyFee > 0)
                await _merchantRepository.RecordMerchantPenaltyTransactionAsync(merchantId, penaltyFee, $"غرامة إلغاء الطلب #{order.Id}");

            // Paid online: the cancelled part goes back to the customer (the whole payment,
            // delivery fee included, when nothing of the order remains).
            decimal refund = 0m;
            if (order.PaymentMethod != PaymentMethod.Cash)
            {
                refund = othersRemain ? ownSum : paidBefore;
                if (refund > 0)
                    await _merchantRepository.RecordTransactionAsync(order.UserId, refund, "Refund", $"استرداد عن الطلب #{order.Id}");
            }

            if (order.UserId > 0)
            {
                await _notificationService.SendNotificationAsync(new SendNotification
                {
                    UserId = order.UserId,
                    Title = "اعتذار عن جزء من الطلب ❌",
                    Message = othersRemain
                        ? $"نعتذر لك، {storeName} ألغى أصنافه من طلبك #{order.Id} وباقي الطلب مستمر."
                        : $"نعتذر لك، تم إلغاء طلبك #{order.Id} من قبل المتجر. يمكنك اختيار متجر آخر."
                });
                if (refund > 0)
                {
                    await _notificationService.SendNotificationAsync(new SendNotification
                    {
                        UserId = order.UserId,
                        Title = "تم رد مبلغ إلى حسابك",
                        Message = $"تم رد {refund:0.##} ج.م عن الطلب #{order.Id} إلى وسيلة الدفع."
                    });
                }
            }

            return true;
        }

        public async Task<MerchantProfileResponseDto?> GetMerchantProfileAsync(int merchantId)
        {
            var merchant = await _merchantRepository.GetMerchantByIdAsync(merchantId);
            if (merchant == null) return null;

            var orders = await _merchantRepository.GetMerchantOrdersAsync(merchantId);
            int activeProducts = await _merchantRepository.GetActiveProductsCountAsync(merchantId);
            // مبيعات وعديد طلبات اليوم المكتملة
            var todayOrders = orders.Where(o => o.CreatedAt.Date == DateTime.UtcNow.Date && o.Status == OrderStatus.Delivered).ToList();
            decimal todaySales = todayOrders.Sum(o => TotalFor(o, merchantId));

            return new MerchantProfileResponseDto
            {
                MerchantId = merchant.Id,
                StoreName = merchant.BusinessName ?? merchant.UserName ?? "متجر الشريك",
                StoreDescription = merchant.BusinessDescription ?? "مطبح المشويات الأصيلة والأكلات الشرقية",
                BranchAddress = merchant.Address ?? "الفرع الرئيسي - شارع مصر",
                ProfileImageUrl = merchant.ProfileImageUrl ?? "",
                Latitude = merchant.Latitude,
                Longitude = merchant.Longitude,
                HasLocation = IsValidLocation(merchant.Latitude, merchant.Longitude),
                IsVerified = true,
                IsOpenForOrders = !merchant.IsFrozen && !merchant.IsPaused,
                WorkHours = string.IsNullOrEmpty(merchant.WorkHours) ? "11:00 ص - 02:00 ص" : merchant.WorkHours,
                // Real figure: 0 until the store has a rating (the app then shows a dash).
                Rating = merchant.Rating,
                TotalReviewsCount = 0,
                SupervisorName = $"{merchant.FirstName} {merchant.LastName}".Trim(),
                AvgPreparationTime = merchant.DeliveryTimeSpan ?? "18 دقيقة",
                TodaySales = todaySales,
                TodayOrdersCount = todayOrders.Count,
                ActiveProductsCount = activeProducts
            };
        }

        /// <summary>A real position: inside the world's range and not the (0,0) default.</summary>
        public static bool IsValidLocation(double lat, double lng)
            => !double.IsNaN(lat) && !double.IsNaN(lng)
               && lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180
               && !(lat == 0 && lng == 0);

        public async Task<bool> UpdateLocationAsync(int merchantId, double latitude, double longitude)
        {
            var merchant = await _merchantRepository.GetMerchantByIdAsync(merchantId);
            if (merchant == null) return false;

            merchant.Latitude = latitude;
            merchant.Longitude = longitude;
            await _merchantRepository.UpdateMerchantAsync(merchant);
            return true;
        }

        public async Task<bool> UpdateWorkHoursAsync(int merchantId, string workHours)
        {
            var merchant = await _merchantRepository.GetMerchantByIdAsync(merchantId);
            if (merchant == null) return false;

            merchant.WorkHours = workHours;
            await _merchantRepository.UpdateMerchantAsync(merchant);
            return true;
        }

        public async Task<StoreMenuDto?> GetStoreMenuAsync(int merchantId)
        {
            var merchant = await _merchantRepository.GetMerchantByIdAsync(merchantId);
            if (merchant == null || merchant.UserType != UserType.Merchant) return null;

            var sections = await _merchantRepository.GetMenuSectionsWithActiveProductsAsync(merchantId);
            var store = new StoreMenuDto
            {
                MerchantId = merchant.Id,
                BusinessName = merchant.BusinessName ?? $"{merchant.FirstName} {merchant.LastName}".Trim(),
                BusinessDescription = merchant.BusinessDescription ?? string.Empty,
                BusinessType = merchant.BusinessType ?? string.Empty,
                MerchantImageUrl = merchant.ProfileImageUrl ?? merchant.Images?.FirstOrDefault()?.Url,
                Address = merchant.Address ?? string.Empty,
                // Same flag the merchant toggles with ToggleStoreStatus.
                IsOpen = !merchant.IsFrozen && !merchant.IsPaused,
                WorkHours = merchant.WorkHours,
                Rating = merchant.Rating > 0 ? (decimal?)merchant.Rating : null,
                Sections = sections.Select(s => new StoreMenuSectionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ProductCount = s.Products?.Count ?? 0,
                    Items = (s.Products ?? new List<Product>()).Select(p => ToCard(p, merchant)).ToList()
                }).ToList()
            };
            return store;
        }

        public async Task<IEnumerable<ProductCardDto>> GetSectionItemsAsync(int merchantId, int sectionId)
        {
            var merchant = await _merchantRepository.GetMerchantByIdAsync(merchantId);
            var products = await _merchantRepository.GetMerchantSectionProductsAsync(merchantId, sectionId);
            return products.Select(p => ToCard(p, merchant)).ToList();
        }

        private static ProductCardDto ToCard(Product p, User? seller) => new ProductCardDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description ?? string.Empty,
            Price = p.Price,
            DiscountedPrice = p.Discount,
            Currency = "EGP",
            Location = p.Location ?? string.Empty,
            ImageUrl = p.Image?.Url ?? p.ImageUrl ?? string.Empty,
            Condition = p.Condition.ToString(),
            UserId = p.UserId,
            SellerName = seller?.BusinessName ?? $"{seller?.FirstName} {seller?.LastName}".Trim(),
            IsSellerVerified = seller?.IsVerified ?? false,
            Stock = p.Stock > 0,
            CreatedAt = p.CreatedAt
        };

        public async Task<IEnumerable<MerchantCardDto>> GetMerchantsByBusinessTypeAsync(string businessType)
        {

            var merchants = await _merchantRepository.GetMerchantsByBusinessTypeAsync(businessType);

            return merchants.Select(m => new MerchantCardDto
            {
                MerchantId = m.Id,
                BusinessName = m.BusinessName ?? $"{m.FirstName} {m.LastName}",
                BusinessDescription = m.BusinessDescription ?? "",
                BusinessType = m.BusinessType,
                MerchantImageUrl = m.Images?.FirstOrDefault()?.Url ?? "",
                Address = m.Address ?? "",
                IsOpen = !m.IsFrozen && !m.IsPaused,
                WorkHours = m.WorkHours ?? "11:00 - 23:00",
                Rating = m.Rating > 0 ? (decimal)m.Rating : 0m,
                ReviewsCount = 0
            }).ToList();
        }
    }
}
    