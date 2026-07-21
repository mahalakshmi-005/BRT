using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BRT.Data;
using BRT.Models;

namespace BRT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContentController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ContentController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.Banners = await _context.Banners.OrderBy(b => b.DisplayOrder).ToListAsync();
            ViewBag.Testimonials = await _context.Testimonials.OrderBy(t => t.DisplayOrder).ToListAsync();
            ViewBag.FAQs = await _context.FAQs.OrderBy(f => f.DisplayOrder).ToListAsync();
            ViewBag.Gallery = await _context.GalleryImages.OrderBy(g => g.DisplayOrder).ToListAsync();
            ViewBag.ContactMessages = await _context.ContactMessages.OrderByDescending(c => c.SubmittedAt).Take(20).ToListAsync();
            return View();
        }

        // --- Banner ---
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBanner(Banner model)
        {
            _context.Banners.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var b = await _context.Banners.FindAsync(id);
            if (b != null) { _context.Banners.Remove(b); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        // --- Testimonial ---
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTestimonial(Testimonial model)
        {
            _context.Testimonials.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTestimonial(int id)
        {
            var t = await _context.Testimonials.FindAsync(id);
            if (t != null) { t.IsApproved = true; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTestimonial(int id)
        {
            var t = await _context.Testimonials.FindAsync(id);
            if (t != null) { _context.Testimonials.Remove(t); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        // --- FAQ ---
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFAQ(FAQ model)
        {
            _context.FAQs.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFAQ(int id)
        {
            var f = await _context.FAQs.FindAsync(id);
            if (f != null) { _context.FAQs.Remove(f); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        // --- Gallery ---
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGalleryImage(GalleryImage model)
        {
            _context.GalleryImages.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGalleryImage(int id)
        {
            var g = await _context.GalleryImages.FindAsync(id);
            if (g != null) { _context.GalleryImages.Remove(g); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        // --- Contact messages ---
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveContact(int id)
        {
            var c = await _context.ContactMessages.FindAsync(id);
            if (c != null) { c.IsResolved = true; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}
