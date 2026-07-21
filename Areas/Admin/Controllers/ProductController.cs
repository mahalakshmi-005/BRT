using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BRT.Data;
using BRT.Models;
using BRT.Services;

namespace BRT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUpload;
        public ProductController(ApplicationDbContext context, IFileUploadService fileUpload)
        {
            _context = context;
            _fileUpload = fileUpload;
        }

        public async Task<IActionResult> Index(int? subCategoryId)
        {
            var query = _context.Products
                .Include(p => p.SubCategory).ThenInclude(s => s!.Category)
                .Include(p => p.PackingTypes)
                .AsQueryable();

            if (subCategoryId.HasValue)
                query = query.Where(p => p.SubCategoryId == subCategoryId);

            ViewBag.SubCategories = new SelectList(await _context.SubCategories.ToListAsync(), "Id", "Name", subCategoryId);
            return View(await query.OrderBy(p => p.Name).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadSubCategories();
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) { await LoadSubCategories(model.SubCategoryId); return View(model); }

            try
            {
                var uploadedUrl = await _fileUpload.SaveImageAsync(imageFile, "products");
                if (uploadedUrl != null) model.ImageUrl = uploadedUrl;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadSubCategories(model.SubCategoryId);
                return View(model);
            }

            model.Slug = await EnsureUniqueProductSlugAsync(Slugify(model.Name));
            model.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product created. Now add packing types.";
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.PackingTypes)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            await LoadSubCategories(product.SubCategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) { await LoadSubCategories(model.SubCategoryId); return View(model); }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            try
            {
                var uploadedUrl = await _fileUpload.SaveImageAsync(imageFile, "products");
                if (uploadedUrl != null)
                {
                    _fileUpload.DeleteImage(product.ImageUrl);
                    product.ImageUrl = uploadedUrl;
                }
                else if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                {
                    product.ImageUrl = model.ImageUrl; // allow pasting a URL instead of uploading
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadSubCategories(model.SubCategoryId);
                return View(model);
            }

            if (!string.Equals(product.Name, model.Name, StringComparison.OrdinalIgnoreCase))
                product.Slug = await EnsureUniqueProductSlugAsync(Slugify(model.Name), excludeId: id);

            product.Name = model.Name;
            product.NameTamil = model.NameTamil;
            product.SubCategoryId = model.SubCategoryId;
            product.Grade = model.Grade;
            product.Description = model.Description;
            product.HSNCode = model.HSNCode;
            product.IsLooseAvailable = model.IsLooseAvailable;
            product.IsActive = model.IsActive;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product updated.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                product.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product removed.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- Packing Types (added inline from Product Edit page) ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPackingType(PackingType model)
        {
            model.CreatedAt = DateTime.UtcNow;
            _context.PackingTypes.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Packing type added.";
            return RedirectToAction(nameof(Edit), new { id = model.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePackingType(int id, int productId)
        {
            var pt = await _context.PackingTypes.FindAsync(id);
            if (pt != null)
            {
                pt.IsDeleted = true;
                pt.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        private async Task LoadSubCategories(int? selected = null)
        {
            var subs = await _context.SubCategories
                .Include(s => s.Category)
                .OrderBy(s => s.Category!.Name).ThenBy(s => s.Name)
                .ToListAsync();
            ViewBag.SubCategoryList = new SelectList(
                subs.Select(s => new { s.Id, Name = $"{s.Category!.Name} → {s.Name}" }),
                "Id", "Name", selected);
        }

        private async Task<string> EnsureUniqueProductSlugAsync(string baseSlug, int? excludeId = null)
        {
            var slug = baseSlug;
            var i = 2;
            while (await _context.Products.IgnoreQueryFilters().AnyAsync(p => p.Slug == slug && p.Id != (excludeId ?? -1)))
                slug = $"{baseSlug}-{i++}";
            return slug;
        }

        private static string Slugify(string name) =>
            name.Trim().ToLowerInvariant().Replace(" ", "-").Replace("&", "and");
    }
}
