using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BRT.Data;
using BRT.Models;

namespace BRT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index(OrderStatus? status)
        {
            var query = _context.OrderRequests.Include(o => o.Items).AsQueryable();
            if (status.HasValue) query = query.Where(o => o.Status == status);

            ViewBag.CurrentFilter = status;
            return View(await query.OrderByDescending(o => o.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.OrderRequests
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.Items).ThenInclude(i => i.PackingType)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus, string? remarks, DateTime? expectedDispatchDate)
        {
            var order = await _context.OrderRequests.FindAsync(id);
            if (order == null) return NotFound();

            var oldStatus = order.Status;
            order.Status = newStatus;
            order.AdminRemarks = remarks;
            order.ExpectedDispatchDate = expectedDispatchDate;
            order.UpdatedAt = DateTime.UtcNow;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderRequestId = id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = User.Identity?.Name,
                Remarks = remarks,
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Order {order.OrderNumber} marked as {newStatus}.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
