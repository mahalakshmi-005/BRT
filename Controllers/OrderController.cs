using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BRT.Data;
using BRT.Models;
using BRT.ViewModels;

namespace BRT.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Request(int? productId)
        {
            await LoadProducts();
            var model = new OrderRequestViewModel();
            if (productId.HasValue) model.Items[0].ProductId = productId;
            ViewData["Title"] = "Request Bulk Order";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(OrderRequestViewModel model)
        {
            var validItems = model.Items.Where(i => i.ProductId.HasValue && i.Quantity is > 0).ToList();
            if (validItems.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one product with quantity.");

            if (!ModelState.IsValid)
            {
                await LoadProducts();
                return View(model);
            }

            var today = DateTime.UtcNow.Date;
            var order = new OrderRequest
            {
                OrderNumber = $"BRT-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                BuyerName = model.BuyerName,
                BusinessName = model.BusinessName,
                PhoneNumber = model.PhoneNumber,
                BuyerAddress = model.BuyerAddress,
                City = model.City,
                Status = OrderStatus.Pending,
                TransportChargeApplicable = true,
                CreatedAt = DateTime.UtcNow
            };

            decimal total = 0;
            foreach (var item in validItems)
            {
                var price = await _context.MarketPrices
                    .Where(m => m.ProductId == item.ProductId && m.PriceDate == today)
                    .Select(m => (decimal?)m.TodayPrice)
                    .FirstOrDefaultAsync() ?? 0;

                var lineItem = new OrderRequestItem
                {
                    ProductId = item.ProductId!.Value,
                    PackingTypeId = item.PackingTypeId,
                    Quantity = item.Quantity!.Value,
                    PriceAtOrderTime = price
                };
                total += lineItem.Quantity * price;
                order.Items.Add(lineItem);
            }
            order.TotalEstimatedAmount = total;

            _context.OrderRequests.Add(order);
            await _context.SaveChangesAsync();

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderRequestId = order.Id,
                OldStatus = OrderStatus.Pending,
                NewStatus = OrderStatus.Pending,
                ChangedBy = "System",
                Remarks = "Order request submitted",
                ChangedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Success), new { orderNumber = order.OrderNumber });
        }

        public IActionResult Success(string orderNumber)
        {
            ViewBag.OrderNumber = orderNumber;
            ViewData["Title"] = "Order Submitted";
            return View();
        }

        private async Task LoadProducts()
        {
            var products = await _context.Products
                .Include(p => p.PackingTypes)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
            ViewBag.ProductList = new SelectList(products, "Id", "Name");
            ViewBag.Products = products;
        }
    }
}
