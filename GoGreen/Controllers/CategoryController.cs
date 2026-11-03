using Berwah.BL.Service;
using Berwah.BL.Service.IService;
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
        private readonly ICategoryService _categoryService;

        public CategoryController(ApplicationDbContext context, ICategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
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

            //var hasProducts = await _context.Products.AnyAsync(p => p.SubCategoryId == id);
            //if (hasProducts)
            //{
            //    return BadRequest(new { status = "fail", message = "لا يمكن حذف قسم فرعي توجد منتجات مرتبطة به" });
            //}

            _context.SubCategories.Remove(subCategory);
            await _context.SaveChangesAsync();

            return Ok(new { status = "success", message = "تم حذف القسم الفرعي بنجاح" });
        }


        /// <summary>Home search: products, approved ads and stores whose name matches the text.</summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] int take = 20)
        {
            q = (q ?? "").Trim();
            if (q.Length < 2)
                return Ok(new { status = "success", data = new { products = new object[0], ads = new object[0], merchants = new object[0], categories = new object[0] } });
            if (take < 1 || take > 50) take = 20;

            var products = await _context.Products
                .Where(p => p.IsActive && !p.User.IsFrozen
                    && (p.Name.Contains(q) || p.Description.Contains(q)))
                .OrderByDescending(p => p.CreatedAt)
                .Take(take)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    Price = p.Price,
                    DiscountedPrice = p.Discount != null && p.Discount > 0 ? (decimal?)(p.Price - p.Discount.Value) : null,
                    p.Currency,
                    p.ImageUrl,
                    MerchantId = p.UserId,
                    SellerName = p.User.BusinessName,
                    MerchantType = p.User.BusinessType,
                    InStock = p.Stock > 0
                })
                .ToListAsync();

            var ads = await _context.Advertisements
                .Where(a => a.IsActive && a.Status == Berwah.Models.Enum.AdStatus.Approved
                    && (a.Title.Contains(q) || a.Description.Contains(q)))
                .OrderByDescending(a => a.CreatedAt)
                .Take(take)
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

            var merchants = await _context.Users
                .Where(u => u.BusinessName != null && !u.IsFrozen && u.BusinessName.Contains(q))
                .Take(take)
                .Select(u => new
                {
                    MerchantId = u.Id,
                    BusinessName = u.BusinessName,
                    BusinessType = u.BusinessType,
                    ImageUrl = u.ProfileImageUrl
                })
                .ToListAsync();

            // Sections and sub-sections by name, so "خدمات" or "موبايلات" lead straight to them.
            var categories = await _context.Categories
                .Where(c => c.Name.Contains(q))
                .Take(take)
                .Select(c => new { c.Id, c.Name, ParentName = (string?)null })
                .ToListAsync();
            var subs = await _context.SubCategories
                .Where(s => s.Name.Contains(q))
                .Take(take)
                .Select(s => new { Id = s.CategoryId, s.Name, ParentName = (string?)s.Category.Name })
                .ToListAsync();
            categories.AddRange(subs);

            return Ok(new { status = "success", data = new { products, ads, merchants, categories } });
        }

        [HttpGet("{id}/content")]
        public async Task<IActionResult> GetCategoryContent(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _categoryService.GetCategoryContentAsync(id, page, pageSize);
            if (result == null)
            {
                return NotFound(new { status = "fail", message = "القسم غير موجود" });
            }

            return Ok(new
            {
                status = "success",
                data = result
            });
        }

        [HttpGet("{categoryId}/filter")]
        public async Task<IActionResult> FilterCategoryContent(
                int categoryId,
    [FromQuery] int? subCategoryId,
    [FromQuery] string? condition, 
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _categoryService.GetFilteredAdsAsync(categoryId, subCategoryId, condition, page, pageSize);
                return Ok(new
                {
                    status = "success",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}
