using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BRT.Data;
using BRT.Models;

namespace BRT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SettingsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            return View(await _context.SiteSettings.OrderBy(s => s.Key).ToListAsync());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(List<int> ids, List<string> values)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                var setting = await _context.SiteSettings.FindAsync(ids[i]);
                if (setting != null) setting.Value = values[i];
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Settings updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSetting(string key, string value)
        {
            if (!await _context.SiteSettings.AnyAsync(s => s.Key == key))
            {
                _context.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
