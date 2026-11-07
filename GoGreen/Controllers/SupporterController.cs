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
    public class SupporterController : ControllerBase
    {
        private readonly ISupporterService _supporterService;

        public SupporterController(ISupporterService supporterService)
        {
            _supporterService = supporterService;
        }

        // POST: api/Supporter/CreateTicket (نموذج رفع تذكرة جديد)
        [HttpPost("CreateTicket")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicket dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = int.TryParse(userIdClaim, out var id) ? id : (int?)null;

            var result = await _supporterService.CreateTicketAsync(userId, dto);
            return Ok(new { status = "success", message = "تم إنشاء التذكرة بنجاح", data = result });
        }

        // GET: api/Supporter/MyTickets (جلب تذاكري السابقة)
        [HttpGet("MyTickets")]
        public async Task<IActionResult> MyTickets()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _supporterService.GetMyTicketsAsync(userId);
            return Ok(new { status = "success", data = result });
        }

        // GET: api/Supporter/Faqs (الأسئلة الأكثر شيوعاً)
        [HttpGet("Faqs")]
        public async Task<IActionResult> Faqs()
        {
            var result = await _supporterService.GetFaqsAsync();
            return Ok(new { status = "success", data = result });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateFaq")]
        public async Task<IActionResult> CreateFaq([FromBody] CreateFaqDto dto)
        {
            var result = await _supporterService.CreateFaqAsync(dto);
            return Ok(new { status = "success", message = "تم إنشاء السؤال الشائع بنجاح", data = result });
        }
    }
}