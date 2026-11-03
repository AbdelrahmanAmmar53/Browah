using Berwah.DAL.App;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("Admin/UpdateCommissionSettings")]
        public async Task<IActionResult> UpdateCommissionSettings([FromBody] UpdateSettingsDto model)
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SystemSetting();
                _context.SystemSettings.Add(settings);
            }

            // الأدمن يحدد الرسوم والنسبة الجديدة
            settings.WithdrawalFeeFixed = model.WithdrawalFeeFixed;
            settings.CompanyCommissionPercentage = model.CompanyCommissionPercentage;
            if (model.CourierSharePercentage.HasValue) settings.CourierSharePercentage = model.CourierSharePercentage.Value;

            await _context.SaveChangesAsync();

            return Ok(new { status = "success", message = "تم تحديث رسوم وعمولات النظام بنجاح" });
        }

        public class UpdateSettingsDto
        {
            public decimal WithdrawalFeeFixed { get; set; } 
            public decimal CompanyCommissionPercentage { get; set; }
            /// <summary>Optional: the captain's share of each order, in percent.</summary>
            public decimal? CourierSharePercentage { get; set; }
        }
    }
}
