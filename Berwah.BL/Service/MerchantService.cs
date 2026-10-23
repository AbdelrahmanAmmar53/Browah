using Berwah.BL.Service.IService;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.DTO;
using Berwah.Models.DTO.Merchant;
using Berwah.Models.Enum;
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

            int totalActive = allOrders.Count(o => o.Status != OrderStatus.Delivered);
            int cookingCount = allOrders.Count(o => o.Status == OrderStatus.Processing);
            int readyCount = allOrders.Count(o => o.Status == OrderStatus.Shipped || (o.DeliveryId != null && o.Status == OrderStatus.Processing));

            decimal todaySales = allOrders
                .Where(o => o.CreatedAt.Date == DateTime.UtcNow.Date && o.Status == OrderStatus.Delivered)
                .Sum(o => (decimal)o.TotalPrice);

            var filteredOrders = filter?.ToLower() switch
            {
                "new" => allOrders.Where(o => o.Status == OrderStatus.Pending).ToList(),
                "cooking" => allOrders.Where(o => o.Status == OrderStatus.Processing).ToList(),
                "ready" => allOrders.Where(o => o.DeliveryId != null).ToList(),
                _ => allOrders
            };

            var orderDtos = filteredOrders.Select(o => new MerchantOrderDto
            {
                OrderId = o.Id,
                OrderNumber = $"#ORD-{o.Id}",
                CustomerName = $"{o.User?.FirstName} {o.User?.LastName}".Trim(),
                CustomerAddress = o.User?.Address ?? "العنوان المحدد",
                TotalPrice = (decimal)o.TotalPrice,
                Status = o.Status.ToString(),
                StatusBadgeText = o.Status == OrderStatus.Pending ? "طلب جديد" : "قيد التجهيز",
                PaymentMethod = o.PaymentMethod == PaymentMethod.Cash ? "نقداً (كاش)" : "دفع إلكتروني",
                Items = o.OrderItems.Select(oi => new MerchantOrderItemDto
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
                IsOpenForOrders = !merchant.IsFrozen,
                TotalActiveOrders = totalActive,
                CookingOrAssemblyOrders = cookingCount,
                ReadyForDeliveryOrders = readyCount,
                TodaySales = todaySales,
                Orders = orderDtos
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, int merchantId, UpdateMerchantOrderStatusDto model)
        {
            var order = await _merchantRepository.GetMerchantOrderWithDetailsAsync(orderId, merchantId);
            if (order == null) return false;

            if (model.NewStatus == "Cooking")
            {
                order.Status = OrderStatus.Processing;
                order.ConditionStaus = "قيد الطهي والتجهيز";
            }
            else if (model.NewStatus == "Ready")
            {
                order.Status = OrderStatus.Shipped;
                order.ConditionStaus = "جاهز لتسليم الوجبات للمندوب";
            }
            else if (model.NewStatus == "Cancelled")
            {
                order.Status = OrderStatus.Cancelled;
                order.ConditionStaus = "تم إلغاء الطلب من قبل المتجر";
            }

            await _merchantRepository.UpdateOrderAsync(order);

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
            await _merchantRepository.UpdateMerchantAsync(merchant);
            return true;
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
                CustomerAddress = order.User?.Address ?? "العنوان المحدد",
                TotalPrice = (decimal)order.TotalPrice,
                Items = order.OrderItems.Select(oi => new MerchantOrderItemDto
                {
                    ProductName = oi.Product?.Name ?? "منتج",
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList(),
                PenaltyAmount = 35.00m, 
                AcceptanceRateImpact = "-2.5%",
                AppearancePenaltyDuration = "4 ساعات"
            };
        }
        public async Task<bool> ConfirmOrderRejectionAsync(int orderId, int merchantId, ConfirmRejectOrderDto model)
        {
            var order = await _merchantRepository.GetMerchantOrderWithDetailsAsync(orderId, merchantId);
            if (order == null) return false;

            decimal penaltyFee = 35.00m;

            order.Status = OrderStatus.Cancelled;
            order.ConditionStaus = $"تم إلغاء الطلب من قبل التاجر (السبب: {model.RejectionReason})";
            await _merchantRepository.UpdateOrderAsync(order);


            await _merchantRepository.RecordMerchantPenaltyTransactionAsync(merchantId, penaltyFee, $"غرامة إلغاء الطلب #{order.Id}");

            if (order.UserId > 0)
            {
                await _notificationService.SendNotificationAsync(new SendNotification
                {
                    UserId = order.UserId,
                    Title = "اعتذار عن الطلب ❌",
                    Message = $"نعتذر لك، تم إلغاء طلبك #{order.Id} من قبل المتجر. يمكنك اختيار متجر آخر."
                });
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
            decimal todaySales = todayOrders.Sum(o => (decimal)o.TotalPrice);

            return new MerchantProfileResponseDto
            {
                MerchantId = merchant.Id,
                StoreName = merchant.BusinessName ?? merchant.UserName ?? "متجر الشريك",
                StoreDescription = merchant.BusinessDescription ?? "مطبح المشويات الأصيلة والأكلات الشرقية",
                BranchAddress = merchant.Address ?? "الفرع الرئيسي - شارع مصر",
                ProfileImageUrl = merchant.ProfileImageUrl ?? "",
                IsVerified = true,
                IsOpenForOrders = !merchant.IsFrozen,
                WorkHours = string.IsNullOrEmpty(merchant.WorkHours) ? "11:00 ص - 02:00 ص" : merchant.WorkHours,
                Rating = 4.9,
                TotalReviewsCount = 1200,
                SupervisorName = $"{merchant.FirstName} {merchant.LastName}".Trim(),
                AvgPreparationTime = merchant.DeliveryTimeSpan ?? "18 دقيقة",
                TodaySales = todaySales,
                TodayOrdersCount = todayOrders.Count,
                ActiveProductsCount = activeProducts
            };
        }

        public async Task<bool> UpdateWorkHoursAsync(int merchantId, string workHours)
        {
            var merchant = await _merchantRepository.GetMerchantByIdAsync(merchantId);
            if (merchant == null) return false;

            merchant.WorkHours = workHours;
            await _merchantRepository.UpdateMerchantAsync(merchant);
            return true;
        }

        public async Task<IEnumerable<MerchantCardDto>> GetMerchantsByBusinessTypeAsync(string businessType)
        {

            var merchants = await _merchantRepository.GetMerchantsByBusinessTypeAsync(businessType);

            return merchants.Select(m => new MerchantCardDto
            {
                Id = m.Id,
                BusinessName = m.BusinessName ?? $"{m.FirstName} {m.LastName}",
                BusinessDescription = m.BusinessDescription ?? "",
                BusinessType = m.BusinessType,
                MerchantImageUrl = m.Images?.FirstOrDefault()?.Url ?? "",
                Address = m.Address ?? "",
                IsOpen = m.AcceptOrdersAutomatically
            }).ToList();
        }
    }
}
    