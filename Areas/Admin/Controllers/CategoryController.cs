using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BRT.Data;
using BRT.Models;
using BRT.Services;

namespace BRT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUpload;
        public CategoryController(ApplicationDbContext context, IFileUploadService fileUpload)
        {
            _context = context;
            _fileUpload = fileUpload;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory() => View(new Category());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var uploadedUrl = await _fileUpload.SaveImageAsync(imageFile, "categories");
                if (uploadedUrl != null) model.ImageUrl = uploadedUrl;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            model.Slug = await EnsureUniqueCategorySlugAsync(Slugify(model.Name));
            model.CreatedAt = DateTime.UtcNow;
            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            try
            {
                var uploadedUrl = await _fileUpload.SaveImageAsync(imageFile, "categories");
                if (uploadedUrl != null)
                {
                    _fileUpload.DeleteImage(cat.ImageUrl);
                    cat.ImageUrl = uploadedUrl;
                }
                else if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                {
                    cat.ImageUrl = model.ImageUrl;
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            // Only regenerate the slug if the name actually changed (avoids -2, -3 creeping up on every edit)
            if (!string.Equals(cat.Name, model.Name, StringComparison.OrdinalIgnoreCase))
                cat.Slug = await EnsureUniqueCategorySlugAsync(Slugify(model.Name), excludeId: id);

            cat.Name = model.Name;
            cat.NameTamil = model.NameTamil;
            cat.Type = model.Type;
            cat.DisplayOrder = model.DisplayOrder;
            cat.IsActive = model.IsActive;
            cat.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat != null)
            {
                cat.IsDeleted = true;
                cat.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category removed.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- SubCategory ---

        [HttpGet]
        public async Task<IActionResult> CreateSubCategory(int categoryId)
        {
            var cat = await _context.Categories.FindAsync(categoryId);
            if (cat == null) return NotFound();
            ViewBag.CategoryName = cat.Name;
            return View(new SubCategory { CategoryId = categoryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubCategory(SubCategory model)
        {
            if (!ModelState.IsValid) return View(model);
            model.Slug = await EnsureUniqueSubCategorySlugAsync(Slugify(model.Name));
            model.CreatedAt = DateTime.UtcNow;
            _context.SubCategories.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Sub-category created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditSubCategory(int id)
        {
            var sub = await _context.SubCategories.FindAsync(id);
            if (sub == null) return NotFound();
            return View(sub);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubCategory(int id, SubCategory model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var sub = await _context.SubCategories.FindAsync(id);
            if (sub == null) return NotFound();

            if (!string.Equals(sub.Name, model.Name, StringComparison.OrdinalIgnoreCase))
                sub.Slug = await EnsureUniqueSubCategorySlugAsync(Slugify(model.Name), excludeId: id);

            sub.Name = model.Name;
            sub.NameTamil = model.NameTamil;
            sub.DisplayOrder = model.DisplayOrder;
            sub.IsActive = model.IsActive;
            sub.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Sub-category updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            var sub = await _context.SubCategories.FindAsync(id);
            if (sub != null)
            {
                sub.IsDeleted = true;
                sub.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Sub-category removed.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- Slug helpers: auto-append -2, -3... on collision instead of crashing with a DB constraint error ---

        private async Task<string> EnsureUniqueCategorySlugAsync(string baseSlug, int? excludeId = null)
        {
            var slug = baseSlug;
            var i = 2;
            while (await _context.Categories.IgnoreQueryFilters().AnyAsync(c => c.Slug == slug && c.Id != (excludeId ?? -1)))
                slug = $"{baseSlug}-{i++}";
            return slug;
        }

        private async Task<string> EnsureUniqueSubCategorySlugAsync(string baseSlug, int? excludeId = null)
        {
            var slug = baseSlug;
            var i = 2;
            while (await _context.SubCategories.IgnoreQueryFilters().AnyAsync(s => s.Slug == slug && s.Id != (excludeId ?? -1)))
                slug = $"{baseSlug}-{i++}";
            return slug;
        }

        private static string Slugify(string name) =>
            name.Trim().ToLowerInvariant().Replace(" ", "-").Replace("&", "and");
    }
}
