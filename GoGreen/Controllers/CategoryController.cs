using Berwah.DAL.App;
using Berwah.Models.DTO;
using Berwah.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Berwah.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMainCategories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    //ImageUrl = c.IconUrl,
                    CategoryType = c.CategoryType.ToString() 
                })
                .ToListAsync();

            return Ok(new { status = "success", data = categories });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Createcategory")]
        public async Task<IActionResult> CreateCategory([FromForm] CreateOrUpdateCategory dto)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
                return BadRequest(new { status = "fail", message = "هذا القسم موجود بالفعل" });

            var category = new Category
            {
                Name = dto.Name,
                CategoryType = dto.CategoryType, 
                //IconUrl = dto.IconFile != null ? "مسار_الصورة_المرفوعة" : null
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return Ok(new { status = "success", message = "تمت إضافة القسم الرئيسي بنجاح", categoryId = category.Id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("Updatecategory/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromForm] CreateOrUpdateCategory dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { status = "fail", message = "القسم غير موجود" });

            category.Name = dto.Name;
            category.CategoryType = dto.CategoryType; 

          

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return Ok(new { status = "success", message = "تم تعديل القسم بنجاح" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("Deletecategory/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound(new { status = "fail", message = "القسم غير موجود" });


            if (category.SubCategories.Any())
            {
                return BadRequest(new { status = "fail", message = "لا يمكن حذف قسم يحتوي على أقسام فرعية مرتبطة به" });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { status = "success", message = "تم حذف القسم بنجاح" });
        }


        [HttpGet("{categoryId}/subcategories")]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            var subCategories = await _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => new { sc.Id, sc.Name })
                .ToListAsync();

            return Ok(new { status = "success", data = subCategories });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateSubcategory")]
        public async Task<IActionResult> CreateSubCategory([FromBody] CreateOrUpdateSubCategory dto)
        {
            // التحقق من وجود القسم الرئيسي
            var mainCategoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!mainCategoryExists)
                return BadRequest(new { status = "fail", message = "القسم الرئيسي غير موجود" });

            var subCategory = new SubCategory
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId
            };

            await _context.SubCategories.AddAsync(subCategory);
            await _context.SaveChangesAsync();

            return Ok(new { status = "success", message = "تمت إضافة القسم الفرعي بنجاح", subCategoryId = subCategory.Id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateSubcategory/{id}")]
        public async Task<IActionResult> UpdateSubCategory(int id, [FromBody] CreateOrUpdateSubCategory dto)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null)
                return NotFound(new { status = "fail", message = "القسم الفرعي غير موجود" });

            subCategory.Name = dto.Name;
            subCategory.CategoryId = dto.CategoryId;

            _context.SubCategories.Update(subCategory);
            await _context.SaveChangesAsync();

            return Ok(new { status = "success", message = "تم تعديل القسم الفرعي بنجاح" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("Deletesubcategory/{id}")]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null)
                return NotFound(new { status = "fail", message = "القسم الفرعي غير موجود" });

            var hasProducts = await _context.Products.AnyAsync(p => p.SubCategoryId == id);
            if (hasProducts)
            {
                return BadRequest(new { status = "fail", message = "لا يمكن حذف قسم فرعي توجد منتجات مرتبطة به" });
            }

            _context.SubCategories.Remove(subCategory);
            await _context.SaveChangesAsync();

            return Ok(new { status = "success", message = "تم حذف القسم الفرعي بنجاح" });
        }
    }
}
