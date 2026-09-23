using Berwah.BL.Service.IService;
using Berwah.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
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
