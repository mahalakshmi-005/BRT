using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BRT.Data;
using BRT.Models;

namespace BRT.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ContactController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.Settings = await _context.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
            ViewData["Title"] = "Contact Us";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ContactMessage model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Settings = await _context.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
                ViewData["Title"] = "Contact Us";
                return View("Index");
            }
            model.SubmittedAt = DateTime.UtcNow;
            _context.ContactMessages.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thanks! We'll get back to you shortly.";
            return RedirectToAction(nameof(Index));
        }
    }
}
