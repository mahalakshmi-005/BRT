using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BRT.Data;
using BRT.Models;

namespace BRT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.PendingOrders = await _context.OrderRequests.CountAsync(o => o.Status == OrderStatus.Pending);
            ViewBag.UnderReviewOrders = await _context.OrderRequests.CountAsync(o => o.Status == OrderStatus.UnderReview);
            ViewBag.ConfirmedOrders = await _context.OrderRequests.CountAsync(o => o.Status == OrderStatus.Confirmed);
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.PricesUpdatedToday = await _context.MarketPrices
                .CountAsync(p => p.PriceDate == DateTime.UtcNow.Date);

            ViewBag.RecentOrders = await _context.OrderRequests
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
