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
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(idClaim);
        }

        public IActionResult Index()
        {
            var userId = GetUserId();

            var cart = _context.Carts
                .FirstOrDefault(c => c.userId == userId);

            var viewModel = new CartViewModel();

            if (cart != null)
            {
                viewModel.Items = _context.CartIItems
                    .Where(ci => ci.CartId == cart.Id)
                    .Select(ci => new CartItemViewModel
                    {
                        CartItemId = ci.Id,
                        ProductId = ci.productId,
                        ProductName = ci.product.Name,
                        ImageUrl = ci.product.ImageUrl,
                        UnitPrice = ci.UnitPrice,
                        Quantity = ci.Quantity,
                        AvailableQuantity = ci.product.AvailableQuantity
                    })
                    .ToList();
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddToCart(Guid productId, int quantity = 1)
        {
            var userId = GetUserId();

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = _context.Carts.FirstOrDefault(c => c.userId == userId);
            if (cart == null)
            {
                cart = new Cart { Id = Guid.NewGuid(), userId = userId };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var existingItem = _context.CartIItems
                .FirstOrDefault(ci => ci.CartId == cart.Id && ci.productId == productId);

            int currentQtyInCart = existingItem?.Quantity ?? 0;

            // Prevent ordering more than what's available in stock
            if (currentQtyInCart + quantity > product.AvailableQuantity)
            {
                TempData["CartError"] = "Not enough stock available for the requested quantity.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _context.CartIItems.Add(new CartIItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    productId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                });
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(Guid cartItemId, int quantity)
        {
            var item = _context.CartIItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (item == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                _context.CartIItems.Remove(item);
            }
            else
            {
                var product = _context.Products.First(p => p.Id == item.productId);
                if (quantity > product.AvailableQuantity)
                {
                    TempData["CartError"] = "Not enough stock available for the requested quantity.";
                    return RedirectToAction("Index");
                }

                item.Quantity = quantity;
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveItem(Guid cartItemId)
        {
            var item = _context.CartIItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (item != null)
            {
                _context.CartIItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}