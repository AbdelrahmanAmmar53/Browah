using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.Enum;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Berwah.Controllers
{
    /// <summary>
    /// The choices inside a service: vehicle types for "وصلني", kinds of work for "صيانة".
    /// Read by the request form; managed by an admin.
    /// </summary>
    [Route("api/Home")]
    [ApiController]
    public class ServiceOptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ServiceOptionsController(ApplicationDbContext context) => _context = context;

        private static readonly string[] RideTypes =
            { "مشوار المطار", "طلب توكتوك", "سيارة ملاكي", "طلب سوزوكي", "عربية نقل" };

        private static readonly string[] MaintenanceKinds =
            { "سباكة", "كهرباء", "نجارة", "تكييف وتبريد", "دهانات", "أجهزة منزلية" };

        // GET api/Home/service-options/{serviceId}  ->  { status, data: [{ id, name }] }
        [HttpGet("service-options/{serviceId:int}")]
        public async Task<IActionResult> GetOptions(int serviceId)
        {
            await EnsureDefaultsAsync();

            var options = await _context.ServiceOptions
                .AsNoTracking()
                .Where(o => o.SubCategoryId == serviceId && o.IsActive)
                .OrderBy(o => o.SortOrder).ThenBy(o => o.Id)
                .Select(o => new ServiceOptionResponseDto { Id = o.Id, Name = o.Name })
                .ToListAsync();

            return Ok(new { status = "success", data = options });
        }

        // POST api/Home/service-options   { serviceId, name, sortOrder, isActive }
        [Authorize(Roles = "Admin")]
        [HttpPost("service-options")]
        public async Task<IActionResult> Create([FromBody] ServiceOptionUpsertDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { status = "error", message = "اسم الخيار مطلوب" });
            if (!await _context.SubCategories.AnyAsync(s => s.Id == dto.ServiceId))
                return BadRequest(new { status = "error", message = "الخدمة غير موجودة" });

            var option = new ServiceOption
            {
                SubCategoryId = dto.ServiceId,
                Name = dto.Name.Trim(),
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive
            };
            _context.ServiceOptions.Add(option);
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", data = new ServiceOptionResponseDto { Id = option.Id, Name = option.Name } });
        }

        // PUT api/Home/service-options/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("service-options/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceOptionUpsertDto dto)
        {
            var option = await _context.ServiceOptions.FindAsync(id);
            if (option == null) return NotFound(new { status = "error", message = "الخيار غير موجود" });
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { status = "error", message = "اسم الخيار مطلوب" });

            option.Name = dto.Name.Trim();
            option.SortOrder = dto.SortOrder;
            option.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", data = new ServiceOptionResponseDto { Id = option.Id, Name = option.Name } });
        }

        // DELETE api/Home/service-options/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("service-options/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var option = await _context.ServiceOptions.FindAsync(id);
            if (option == null) return NotFound(new { status = "error", message = "الخيار غير موجود" });

            // Requests that used it keep the id; they just lose the name.
            _context.ServiceOptions.Remove(option);
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "تم حذف الخيار" });
        }

        /// <summary>
        /// The very first time (no option exists yet) gives "وصلني" its vehicle types and
        /// "صيانة" its kinds of work, found by name among the Services sub-categories.
        /// After that the admin manages them.
        /// </summary>
        private async Task EnsureDefaultsAsync()
        {
            if (await _context.ServiceOptions.AnyAsync()) return;

            var services = await _context.SubCategories
                .Where(s => s.Category.CategoryType == CategoryType.Services)
                .ToListAsync();

            foreach (var service in services)
            {
                string[]? names = null;
                if (service.Name.Contains("وصلني")) names = RideTypes;
                else if (service.Name.Contains("صيانة")) names = MaintenanceKinds;
                if (names == null) continue;

                for (var i = 0; i < names.Length; i++)
                    _context.ServiceOptions.Add(new ServiceOption { SubCategoryId = service.Id, Name = names[i], SortOrder = i });
            }

            await _context.SaveChangesAsync();
        }
    }
}
