using System.Text;
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

            var sb = new StringBuilder();
            sb.AppendLine("*New Contact Enquiry — BRT Website*");
            sb.AppendLine();
            sb.AppendLine($"Name: {model.Name}");
            sb.AppendLine($"Phone: {model.Phone}");
            if (!string.IsNullOrWhiteSpace(model.Email)) sb.AppendLine($"Email: {model.Email}");
            sb.AppendLine();
            sb.AppendLine($"Message: {model.Message}");
            sb.AppendLine();
            sb.AppendLine("I'd like to know more about your products and wholesale pricing.");

            var adminWhatsApp = await GetAdminWhatsAppNumberAsync();
            var whatsAppUrl = $"https://api.whatsapp.com/send?phone={adminWhatsApp}&text={Uri.EscapeDataString(sb.ToString())}";

            TempData["Success"] = "Thanks! We'll get back to you shortly.";
            TempData["WhatsAppUrl"] = whatsAppUrl;
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GetAdminWhatsAppNumberAsync()
        {
            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "WhatsAppNumber");
            return setting?.Value ?? "919865680694";
        }
    }
}
