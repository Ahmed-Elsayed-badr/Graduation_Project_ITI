using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Graduation_Project_ITI.Models;
using BLL.Configuration;
using DAL;
using System.Linq;
using System.Security.Claims;

namespace Graduation_Project_ITI.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(idClaim);
        }

        // Creates an Order from the current cart, then empties the cart
        [HttpPost]
        public IActionResult Checkout()
        {
            var userId = GetUserId();

            var cart = _context.Carts.FirstOrDefault(c => c.userId == userId);

            var cartItems = cart == null
                ? new List<CartIItem>()
                : _context.CartIItems.Where(ci => ci.CartId == cart.Id).ToList();

            if (!cartItems.Any())
            {
                TempData["CartError"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // Re-check stock for every item before placing the order
            foreach (var item in cartItems)
            {
                var product = _context.Products.First(p => p.Id == item.productId);
                if (item.Quantity > product.AvailableQuantity)
                {
                    TempData["CartError"] = $"Not enough stock for {product.Name} anymore.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                Status = "Pending",
                userId = userId,
                TotalAmount = cartItems.Sum(ci => ci.UnitPrice * ci.Quantity),
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in cartItems)
            {
                var product = _context.Products.First(p => p.Id == item.productId);

                order.OrderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    productId = item.productId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SellerId = product.sellerId
                });

                // Reduce available stock
                product.AvailableQuantity -= item.Quantity;
            }

            _context.Orders.Add(order);

            // Empty the cart
            _context.CartIItems.RemoveRange(cartItems);

            _context.SaveChanges();

            return RedirectToAction("Details", new { id = order.Id });
        }

        // Order history
        public IActionResult Index()
        {
            var userId = GetUserId();

            var orders = _context.Orders
                .Where(o => o.userId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderListViewModel
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    ItemsCount = o.OrderItems.Count()
                })
                .ToList();

            return View(orders);
        }

        public IActionResult Details(Guid id)
        {
            var userId = GetUserId();

            var order = _context.Orders
                .Where(o => o.Id == id && o.userId == userId)
                .Select(o => new OrderDetailsViewModel
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new OrderItemLineViewModel
                    {
                        ProductName = oi.product.Name,
                        ImageUrl = oi.product.ImageUrl,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                })
                .FirstOrDefault();

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Customers can only cancel while the order is still Pending
        [HttpPost]
        public IActionResult Cancel(Guid id)
        {
            var userId = GetUserId();

            var order = _context.Orders
                .FirstOrDefault(o => o.Id == id && o.userId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Pending")
            {
                TempData["OrderError"] = "This order can no longer be cancelled.";
                return RedirectToAction("Details", new { id });
            }

            // Restore stock for each item
            var orderItems = _context.OrderItems.Where(oi => oi.OrderId == order.Id).ToList();
            foreach (var item in orderItems)
            {
                var product = _context.Products.First(p => p.Id == item.productId);
                product.AvailableQuantity += item.Quantity;
            }

            order.Status = "Cancelled";
            _context.SaveChanges();

            return RedirectToAction("Details", new { id });
        }
    }
}