using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Berwah.Services
{
    /// <summary>How the delivery spot sits against a store.</summary>
    public enum DeliveryZone
    {
        /// <summary>A place could not be worked out: the order goes ahead as usual.</summary>
        Unknown = 0,
        SameCity = 1,
        /// <summary>Same governorate, another city: allowed, delivery costs more.</summary>
        OtherCity = 2,
        /// <summary>Another governorate of the same country: not allowed.</summary>
        BlockedGovernorate = 3,
        /// <summary>Another country: not allowed.</summary>
        BlockedCountry = 4
    }

    public record PlaceInfo(string Country, string State, string City);

    /// <summary>
    /// Finds the country, governorate and city of a point (OpenStreetMap's Nominatim, asked in
    /// English so two places are always compared in the same spelling) and compares a store
    /// with the delivery spot. Answers are kept, and a failed lookup never stops an order.
    /// </summary>
    public class GeoZoneService
    {
        /// <summary>The delivery fee covers this many km between the store and the customer.</summary>
        public const double FreeDistanceKm = 7;

        /// <summary>Each km beyond [FreeDistanceKm] (or part of one) adds this much.</summary>
        public const decimal PricePerExtraKm = 5m;

        /// <summary>Straight-line distance in km between two points.</summary>
        public static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            double Rad(double d) => d * Math.PI / 180;
            var dLat = Rad(lat2 - lat1);
            var dLon = Rad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        /// <summary>The extra delivery fee for [km]: [PricePerExtraKm] for every started km after [FreeDistanceKm].</summary>
        public static decimal ExtraFeeFor(double km) =>
            km <= FreeDistanceKm ? 0m : (decimal)Math.Ceiling(km - FreeDistanceKm) * PricePerExtraKm;

        private static readonly HttpClient Http = CreateClient();
        private static readonly ConcurrentDictionary<string, PlaceInfo?> Cache = new();
        private static readonly SemaphoreSlim Gate = new(1, 1);
        private static DateTime _lastCall = DateTime.MinValue;

        private readonly ILogger<GeoZoneService> _log;
        public GeoZoneService(ILogger<GeoZoneService> log) => _log = log;

        private static HttpClient CreateClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };
            // Nominatim's usage policy asks for an identifying User-Agent.
            client.DefaultRequestHeaders.UserAgent.ParseAdd("BerwahElMazoury-Backend/1.0");
            return client;
        }

        public async Task<PlaceInfo?> ResolveAsync(double latitude, double longitude)
        {
            if (latitude == 0 && longitude == 0) return null;
            if (Math.Abs(latitude) > 90 || Math.Abs(longitude) > 180) return null;

            // About 100 m: points of one block share an answer.
            var key = $"{Math.Round(latitude, 3).ToString(CultureInfo.InvariantCulture)},{Math.Round(longitude, 3).ToString(CultureInfo.InvariantCulture)}";
            if (Cache.TryGetValue(key, out var known)) return known;

            await Gate.WaitAsync();
            try
            {
                if (Cache.TryGetValue(key, out known)) return known;
                // One lookup a second at most.
                var wait = TimeSpan.FromMilliseconds(1100) - (DateTime.UtcNow - _lastCall);
                if (wait > TimeSpan.Zero) await Task.Delay(wait);

                var url = "https://nominatim.openstreetmap.org/reverse?format=jsonv2&zoom=14&addressdetails=1&accept-language=en"
                    + $"&lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}";
                _lastCall = DateTime.UtcNow;
                using var response = await Http.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null; // not cached: try again next time

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                PlaceInfo? place = null;
                if (doc.RootElement.TryGetProperty("address", out var a))
                {
                    string Get(params string[] names)
                    {
                        foreach (var n in names)
                            if (a.TryGetProperty(n, out var v) && v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString()))
                                return v.GetString()!;
                        return "";
                    }
                    var country = Get("country_code").ToLowerInvariant();
                    var state = Get("state", "region", "state_district");
                    var city = Get("city", "town", "village", "municipality", "county", "state_district", "suburb");
                    if (country.Length > 0) place = new PlaceInfo(country, state, city);
                }
                Cache[key] = place;
                return place;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Reverse geocoding failed.");
                return null;
            }
            finally
            {
                Gate.Release();
            }
        }

        private static string Normalize(string value)
        {
            var v = value.ToLowerInvariant();
            v = Regex.Replace(v, @"\b(governorate|province|city|district|markaz|qism|department)\b", " ");
            v = Regex.Replace(v, @"[^a-z0-9]+", "");
            return v;
        }

        /// <summary>The store against the delivery spot. An unknown part never blocks.</summary>
        public static DeliveryZone Compare(PlaceInfo? store, PlaceInfo? dropoff)
        {
            if (store == null || dropoff == null) return DeliveryZone.Unknown;
            if (store.Country != dropoff.Country) return DeliveryZone.BlockedCountry;

            var s = Normalize(store.State);
            var d = Normalize(dropoff.State);
            if (s.Length > 0 && d.Length > 0 && s != d) return DeliveryZone.BlockedGovernorate;

            var sc = Normalize(store.City);
            var dc = Normalize(dropoff.City);
            if (sc.Length > 0 && dc.Length > 0 && sc != dc) return DeliveryZone.OtherCity;
            return DeliveryZone.SameCity;
        }

        /// <summary>The most restrictive zone over all the stores of an order (the enum is ordered that way).</summary>
        public static DeliveryZone Worst(IEnumerable<DeliveryZone> zones) =>
            zones.DefaultIfEmpty(DeliveryZone.Unknown).Max();

        public static string Code(DeliveryZone zone) => zone switch
        {
            DeliveryZone.SameCity => "same_city",
            DeliveryZone.OtherCity => "other_city",
            DeliveryZone.BlockedGovernorate => "blocked_governorate",
            DeliveryZone.BlockedCountry => "blocked_country",
            _ => "unknown"
        };
    }
}
