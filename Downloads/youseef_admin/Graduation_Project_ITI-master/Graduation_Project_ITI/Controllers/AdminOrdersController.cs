using BLL.Configuration;
using Graduation_Project_ITI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project_ITI.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminOrdersController : Controller
    {
        private readonly AppDbContext _context;

        public AdminOrdersController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new AdminOrderViewModel
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    CustomerName = o.user.Name,
                    CustomerEmail = o.user.Email,
                    ItemsCount = o.OrderItems.Sum(i => i.Quantity)
                })
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["AdminError"] = "Invalid order id.";
                return RedirectToAction(nameof(Index));
            }

            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new AdminOrderDetailsViewModel
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    CustomerName = o.user.Name,
                    CustomerEmail = o.user.Email,
                    CustomerPhone = o.user.phonenumber
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                TempData["AdminError"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var items = await _context.OrderItems
                .AsNoTracking()
                .Where(i => i.OrderId == id)
                .Select(i => new
                {
                    i.productId,
                    ProductName = i.product.Name,
                    i.product.ImageUrl,
                    i.Quantity,
                    i.UnitPrice,
                    i.SellerId
                })
                .ToListAsync();

            // OrderItem.SellerId لا يملك Navigation، فنجلب البائعين المعنيين ونطابقهم يدويًا
            var sellerIds = items.Select(i => i.SellerId).Distinct().ToList();

            var sellers = await _context.Sellers
                .AsNoTracking()
                .Where(s => sellerIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.ShopName,
                    UserName = s.user != null ? s.user.Name : null
                })
                .ToDictionaryAsync(s => s.Id);

            order.Items = items.Select(i =>
            {
                sellers.TryGetValue(i.SellerId, out var seller);

                return new AdminOrderItemViewModel
                {
                    ProductId = i.productId,
                    ProductName = i.ProductName,
                    ImageUrl = i.ImageUrl,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    SellerName = seller?.UserName ?? "-",
                    ShopName = seller?.ShopName
                };
            }).ToList();

            return View(order);
        }
    }
}