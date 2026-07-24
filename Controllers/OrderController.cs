using System.Text;
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

            // Use each product's most-recent entered price (not strictly "today") so the
            // order total doesn't silently come out as ₹0 just because Admin hasn't
            // re-entered today's price yet.
            var latestPrices = await GetLatestPricesAsync();

            var lineDescriptions = new List<string>();
            decimal total = 0;
            foreach (var item in validItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId!.Value);
                var price = latestPrices.TryGetValue(item.ProductId.Value, out var mp) ? mp.TodayPrice : 0;

                var lineItem = new OrderRequestItem
                {
                    ProductId = item.ProductId.Value,
                    PackingTypeId = item.PackingTypeId,
                    Quantity = item.Quantity!.Value,
                    PriceAtOrderTime = price
                };
                total += lineItem.Quantity * price;
                order.Items.Add(lineItem);

                lineDescriptions.Add($"- {product?.Name ?? "Product"} x {item.Quantity} (₹{price:F2}/unit)");
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

            // --- Build the WhatsApp message that goes to Admin ---
            var sb = new StringBuilder();
            sb.AppendLine($"*New Bulk Order Request — {order.OrderNumber}*");
            sb.AppendLine();
            sb.AppendLine($"Name: {order.BuyerName}");
            sb.AppendLine($"Business: {order.BusinessName}");
            sb.AppendLine($"Phone: {order.PhoneNumber}");
            if (!string.IsNullOrWhiteSpace(order.City)) sb.AppendLine($"City: {order.City}");
            if (!string.IsNullOrWhiteSpace(order.BuyerAddress)) sb.AppendLine($"Address: {order.BuyerAddress}");
            sb.AppendLine();
            sb.AppendLine("Products:");
            foreach (var line in lineDescriptions) sb.AppendLine(line);
            sb.AppendLine();
            sb.AppendLine($"Estimated Total: ₹{total:F2} (+ transport if applicable)");
            sb.AppendLine("Please confirm pricing and dispatch.");

            var adminWhatsApp = await GetAdminWhatsAppNumberAsync();
            var whatsAppUrl = $"https://api.whatsapp.com/send?phone={adminWhatsApp}&text={Uri.EscapeDataString(sb.ToString())}";

            TempData["WhatsAppUrl"] = whatsAppUrl;
            return RedirectToAction(nameof(Success), new { orderNumber = order.OrderNumber });
        }

        public IActionResult Success(string orderNumber)
        {
            ViewBag.OrderNumber = orderNumber;
            ViewBag.WhatsAppUrl = TempData["WhatsAppUrl"] as string;
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

        // Returns the most recent MarketPrice per product (mirrors CatalogController's logic).
        private async Task<Dictionary<int, MarketPrice>> GetLatestPricesAsync()
        {
            var allPrices = await _context.MarketPrices.ToListAsync();
            return allPrices
                .GroupBy(m => m.ProductId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.PriceDate).ThenByDescending(m => m.UpdatedAt).First());
        }

        private async Task<string> GetAdminWhatsAppNumberAsync()
        {
            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "WhatsAppNumber");
            return setting?.Value ?? "919865680694";
        }
    }
}
