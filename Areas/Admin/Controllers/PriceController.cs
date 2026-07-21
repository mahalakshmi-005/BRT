using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BRT.Data;
using BRT.Models;

namespace BRT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PriceController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PriceController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;

            var products = await _context.Products
                .Include(p => p.SubCategory)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var todaysPrices = await _context.MarketPrices
                .Where(m => m.PriceDate == today)
                .ToListAsync();

            ViewBag.TodaysPrices = todaysPrices.ToDictionary(p => p.ProductId, p => p);
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrice(int productId, decimal todayPrice, bool gstIncluded, string? updatedBy)
        {
            var today = DateTime.UtcNow.Date;
            var existing = await _context.MarketPrices
                .FirstOrDefaultAsync(m => m.ProductId == productId && m.PriceDate == today);

            var lastPrice = await _context.MarketPrices
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.PriceDate)
                .FirstOrDefaultAsync();

            var previousPrice = lastPrice?.TodayPrice ?? todayPrice;

            if (existing != null)
            {
                existing.PreviousPrice = existing.TodayPrice;
                existing.TodayPrice = todayPrice;
                existing.GSTIncluded = gstIncluded;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = updatedBy;
            }
            else
            {
                _context.MarketPrices.Add(new MarketPrice
                {
                    ProductId = productId,
                    TodayPrice = todayPrice,
                    PreviousPrice = previousPrice,
                    GSTIncluded = gstIncluded,
                    PriceDate = today,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = updatedBy
                });
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Price updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Highlights()
        {
            var highlights = await _context.MarketHighlights
                .Include(h => h.Product)
                .OrderByDescending(h => h.DisplayDate)
                .ToListAsync();
            ViewBag.Products = new SelectList(await _context.Products.Where(p => p.IsActive).ToListAsync(), "Id", "Name");
            return View(highlights);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHighlight(int productId, string highlightText, TrendDirection trendDirection)
        {
            _context.MarketHighlights.Add(new MarketHighlight
            {
                ProductId = productId,
                HighlightText = highlightText,
                TrendDirection = trendDirection,
                DisplayDate = DateTime.UtcNow.Date,
                IsActive = true
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Highlight added.";
            return RedirectToAction(nameof(Highlights));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHighlight(int id)
        {
            var h = await _context.MarketHighlights.FindAsync(id);
            if (h != null)
            {
                _context.MarketHighlights.Remove(h);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Highlights));
        }
    }
}
