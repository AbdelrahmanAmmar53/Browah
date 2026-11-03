using Berwah.BL.Service.IService;
using Berwah.DAL.App;
using Berwah.Models.Models;
using Microsoft.EntityFrameworkCore;
using Berwah.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _db;

        public NotificationController(INotificationService notificationService, ApplicationDbContext db)
        {
            _notificationService = notificationService;
            _db = db;
        }

        public class DeviceDto
        {
            public string? Token { get; set; }
            public string? Platform { get; set; }
            public string? Language { get; set; }
        }

        // The app tells the server which phone to wake up for this account.
        [HttpPost("RegisterDevice")]
        public async Task<IActionResult> RegisterDevice([FromBody] DeviceDto dto)
        {
            var token = dto.Token?.Trim();
            if (string.IsNullOrEmpty(token) || token.Length > 512)
                return BadRequest(new { status = "error", message = "بيانات الجهاز غير صحيحة" });
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var me))
                return Unauthorized();

            var platform = dto.Platform == "ios" ? "ios" : "android";
            var language = dto.Language == "en" ? "en" : "ar";
            var row = await _db.DeviceTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (row == null)
            {
                _db.DeviceTokens.Add(new DeviceToken { UserId = me, Token = token, Platform = platform, Language = language, UpdatedAt = DateTime.UtcNow });
            }
            else
            {
                // The same phone signed in with another account: it now belongs to that account.
                row.UserId = me;
                row.Platform = platform;
                row.Language = language;
                row.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
            return Ok(new { status = "success" });
        }

        // Signing out: this phone stops getting this account's notifications.
        [HttpPost("UnregisterDevice")]
        public async Task<IActionResult> UnregisterDevice([FromBody] DeviceDto dto)
        {
            var token = dto.Token?.Trim();
            if (string.IsNullOrEmpty(token) || !int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var me))
                return Ok(new { status = "success" });
            var rows = await _db.DeviceTokens.Where(t => t.Token == token && t.UserId == me).ToListAsync();
            if (rows.Count > 0)
            {
                _db.DeviceTokens.RemoveRange(rows);
                await _db.SaveChangesAsync();
            }
            return Ok(new { status = "success" });
        }

        [HttpGet("HistoryOfNotification")]
        public async Task<IActionResult> GetHistoryNotification()
        {
            // طباعة كل الـ Claims اللي السيرفر قدر يقراها من التوكن
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            if (!userClaims.Any())
            {
                return Unauthorized("السيرفر لم يستلم أي Claims من التوكن (الـ Authorization Header مفقود أو التوكن غير صالح)");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value
                           ?? User.FindFirst("nameid")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { Message = "التوكن قرأ بيانات بس الـ UserId مش موجود", AvailableClaims = userClaims });
            }

            var notifications = await _notificationService.GetNotificationHistoryAsync(userId);

            return Ok(new { status = "success", data = notifications });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("SendNotification")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotification sendNotification)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "Invalid notification data"
                });
            }
            await _notificationService.SendNotificationAsync(sendNotification);
            return Ok(new
            {
                status = "success",
                message = "Notification sent successfully"
            });
        }
    }
}
