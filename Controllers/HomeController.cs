using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BRT.Data;

namespace BRT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;

            ViewBag.Categories = await _context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            ViewBag.Highlights = await _context.MarketHighlights
                .Where(h => h.IsActive)
                .OrderByDescending(h => h.DisplayDate)
                .Take(6)
                .ToListAsync();

            ViewBag.Banners = await _context.Banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();

            ViewBag.Testimonials = await _context.Testimonials
                .Where(t => t.IsApproved)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();

            ViewBag.FAQs = await _context.FAQs
                .Where(f => f.IsActive)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();

            // Featured products for the "Premium Products" showcase — one per sub-category, first active product found
            var activeProducts = await _context.Products
                .Include(p => p.SubCategory).ThenInclude(s => s!.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.SubCategoryId).ThenBy(p => p.Name)
                .ToListAsync();

            ViewBag.FeaturedProducts = activeProducts
                .GroupBy(p => p.SubCategoryId)
                .SelectMany(g => g.Take(2))
                .Take(6)
                .ToList();

            // Live Market Dashboard — most recently updated prices (today's if entered, otherwise latest on file)
            var pricesToday = await _context.MarketPrices
                .Include(m => m.Product).ThenInclude(p => p!.SubCategory).ThenInclude(s => s!.Category)
                .Where(m => m.PriceDate == today && m.Product!.IsActive)
                .OrderByDescending(m => m.UpdatedAt)
                .Take(3)
                .ToListAsync();

            if (pricesToday.Count == 0)
            {
                pricesToday = await _context.MarketPrices
                    .Include(m => m.Product).ThenInclude(p => p!.SubCategory).ThenInclude(s => s!.Category)
                    .Where(m => m.Product!.IsActive)
                    .OrderByDescending(m => m.UpdatedAt)
                    .Take(3)
                    .ToListAsync();
            }
            ViewBag.MarketDashboard = pricesToday;

            return View();
        }

        public IActionResult Error() => View();

        public IActionResult About()
        {
            ViewData["Title"] = "About Us";
            return View();
        }

        public IActionResult WhyChooseUs()
        {
            ViewData["Title"] = "Why Choose Us";
            return View();
        }
    }
}
