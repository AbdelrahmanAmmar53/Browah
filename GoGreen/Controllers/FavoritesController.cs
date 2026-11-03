using Berwah.DAL.App;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Berwah.Controllers
{
    /// <summary>The advertisements a customer hearted, kept on their account so they follow them to any phone.</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public FavoritesController(ApplicationDbContext db) => _db = db;

        private int Me => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>My favourites, the newest first, with what a card shows.</summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var me = Me;
            var ids = await _db.FavoriteAds.AsNoTracking().Where(f => f.UserId == me)
                .OrderByDescending(f => f.CreatedAt).Select(f => f.AdvertisementId).ToListAsync();

            var ads = await _db.Advertisements.AsNoTracking()
                .Where(a => ids.Contains(a.Id))
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Price,
                    a.City,
                    a.Governorate,
                    ImageUrl = a.AdvertismentImages.Select(i => i.Image!.Url).FirstOrDefault()
                })
                .ToListAsync();

            // Keep the order they were hearted in; an ad that was deleted simply drops out.
            var byId = ads.ToDictionary(a => a.Id);
            var data = ids.Where(byId.ContainsKey).Select(i => byId[i]).ToList();
            return Ok(new { status = "success", data });
        }

        [HttpPost("{adId}")]
        public async Task<IActionResult> Add(int adId)
        {
            var me = Me;
            if (!await _db.Advertisements.AnyAsync(a => a.Id == adId))
                return NotFound(new { status = "error", message = "الإعلان غير موجود" });
            if (!await _db.FavoriteAds.AnyAsync(f => f.UserId == me && f.AdvertisementId == adId))
            {
                _db.FavoriteAds.Add(new FavoriteAd { UserId = me, AdvertisementId = adId });
                try { await _db.SaveChangesAsync(); } catch (DbUpdateException) { /* hearted twice at once */ }
            }
            return Ok(new { status = "success" });
        }

        [HttpDelete("{adId}")]
        public async Task<IActionResult> Remove(int adId)
        {
            var me = Me;
            var rows = await _db.FavoriteAds.Where(f => f.UserId == me && f.AdvertisementId == adId).ToListAsync();
            if (rows.Count > 0)
            {
                _db.FavoriteAds.RemoveRange(rows);
                await _db.SaveChangesAsync();
            }
            return Ok(new { status = "success" });
        }

        public class MergeFavoritesDto
        {
            public List<int>? Ids { get; set; }
        }

        /// <summary>Adds the ones hearted on the phone before signing in; returns nothing to do for ids already there.</summary>
        [HttpPost("merge")]
        public async Task<IActionResult> Merge([FromBody] MergeFavoritesDto dto)
        {
            var me = Me;
            var wanted = (dto.Ids ?? new()).Distinct().Take(200).ToList();
            if (wanted.Count == 0) return Ok(new { status = "success" });

            var have = await _db.FavoriteAds.Where(f => f.UserId == me && wanted.Contains(f.AdvertisementId))
                .Select(f => f.AdvertisementId).ToListAsync();
            var real = await _db.Advertisements.Where(a => wanted.Contains(a.Id)).Select(a => a.Id).ToListAsync();
            foreach (var id in real.Except(have))
                _db.FavoriteAds.Add(new FavoriteAd { UserId = me, AdvertisementId = id });
            try { await _db.SaveChangesAsync(); } catch (DbUpdateException) { }
            return Ok(new { status = "success" });
        }
    }
}
