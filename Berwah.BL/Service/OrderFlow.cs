using Berwah.BL.Service.IService;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;

namespace Berwah.BL.Service
{
    /// <summary>
    /// When an order becomes visible to couriers.
    /// Restaurants, groceries and home kitchens must accept their part first; every other store's
    /// items can be taken by a courier right after the customer confirms.
    /// </summary>
    public static class OrderFlow
    {
        private static readonly string[] FoodTypes =
        {
            "مطعم", "مطاعم", "restaurant",
            "بقالة", "البقالة", "grocery",
            "مطبخ بيتي", "اكل بيتي", "أكل بيتي", "homefood"
        };

        /// <summary>A store that has to accept before a courier is called (unknown type counts as food).</summary>
        public static bool IsFoodStore(string? businessType)
        {
            if (string.IsNullOrWhiteSpace(businessType)) return true;
            return FoodTypes.Contains(businessType.Trim().ToLowerInvariant());
        }

        /// <summary>
        /// True when every live (not cancelled) item is either from a store that needs no acceptance, or
        /// its store has accepted it. Needs each item's Product and Product.User loaded.
        /// </summary>
        public static bool IsReadyForCouriers(Order order)
        {
            var live = order.OrderItems.Where(i => i.MerchantStatus != OrderStatus.Cancelled).ToList();
            if (live.Count == 0) return false;
            return live.All(i =>
            {
                if (!IsFoodStore(i.Product?.User?.BusinessType)) return true;
                var st = i.MerchantStatus ?? OrderStatus.Pending;
                return st == OrderStatus.Processing || st == OrderStatus.Shipped;
            });
        }

        public static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double r = 6371;
            double dLat = (lat2 - lat1) * Math.PI / 180, dLon = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return 2 * r * Math.Asin(Math.Sqrt(a));
        }

        /// <summary>Tells every active courier within 10 km of the pickup (all of them when it has no location) about the order.</summary>
        public static async Task NotifyCouriersAsync(IEnumerable<User> couriers, INotificationService notifications, Order order)
        {
            bool hasPickup = order.PickupLatitude != 0 && order.PickupLongitude != 0;
            foreach (var c in couriers)
            {
                if (c.IsUnavailable) continue;
                bool near = !hasPickup || (c.Latitude == 0 && c.Longitude == 0)
                    || DistanceKm(order.PickupLatitude, order.PickupLongitude, c.Latitude, c.Longitude) <= 10;
                if (!near) continue;
                try
                {
                    await notifications.SendNotificationAsync(new SendNotification
                    {
                        UserId = c.Id,
                        Title = "طلب توصيل جديد 🛵",
                        Message = $"يوجد طلب جديد رقم #{order.Id} قريب منك. افتح الرادار لقبوله."
                    });
                }
                catch { /* best effort */ }
            }
        }
    }
}
