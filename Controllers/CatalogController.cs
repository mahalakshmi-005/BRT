using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BRT.Data;
using BRT.Models;

namespace BRT.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CatalogController(ApplicationDbContext context) => _context = context;

        // GET: /Catalog or /Catalog/Index
        public IActionResult Index()
        {
            ViewData["Title"] = "Wholesale Products";
            return View();
        }

        // GET: /Catalog/Garlic
        public async Task<IActionResult> Garlic()
        {
            var data = await LoadCategory(CategoryType.Garlic);
            ViewData["Title"] = "Garlic Wholesale";
            return View("CategoryList", data);
        }

        // GET: /Catalog/Grocery
        public async Task<IActionResult> Grocery()
        {
            var data = await LoadCategory(CategoryType.Grocery);
            ViewData["Title"] = "Grocery Wholesale";
            return View("CategoryList", data);
        }

        // GET: /Catalog/Loose
        public async Task<IActionResult> Loose()
        {
            var products = await _context.Products
                .Include(p => p.SubCategory)
                .Include(p => p.PackingTypes)
                .Include(p => p.MarketPrices)
                .Where(p => p.IsActive && p.IsLooseAvailable)
                .ToListAsync();

            ViewBag.TodaysPrices = await GetLatestPricesAsync();

            ViewData["Title"] = "Retail Products";
            return View(products);
        }

        // GET: /Catalog/Product/{slug}
        public async Task<IActionResult> Product(string slug)
        {
            var product = await _context.Products
                .Include(p => p.SubCategory).ThenInclude(s => s!.Category)
                .Include(p => p.PackingTypes)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

            if (product == null) return NotFound();

            var latestPrices = await GetLatestPricesAsync();
            ViewBag.TodayPrice = latestPrices.TryGetValue(product.Id, out var mp) ? mp : null;

            ViewData["Title"] = product.Name;
            return View(product);
        }

        // Helper Method for Category Parsing
        private async Task<List<SubCategory>> LoadCategory(CategoryType type)
        {
            var subs = await _context.SubCategories
                .Include(s => s.Category)
                .Include(s => s.Products.Where(p => p.IsActive))
                    .ThenInclude(p => p.PackingTypes)
                .Where(s => s.Category!.Type == type && s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            ViewBag.TodaysPrices = await GetLatestPricesAsync();

            return subs;
        }

        // Returns the most recent MarketPrice per product (not strictly "today") so the
        // catalog never goes blank just because Admin hasn't re-entered today's price yet.
        private async Task<Dictionary<int, MarketPrice>> GetLatestPricesAsync()
        {
            var allPrices = await _context.MarketPrices.ToListAsync();
            return allPrices
                .GroupBy(m => m.ProductId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.PriceDate).ThenByDescending(m => m.UpdatedAt).First());
        }
    }
}
