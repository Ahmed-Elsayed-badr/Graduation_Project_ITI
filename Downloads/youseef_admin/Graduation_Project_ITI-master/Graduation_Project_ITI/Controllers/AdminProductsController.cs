using BLL.Configuration;
using Graduation_Project_ITI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project_ITI.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminProductsController : Controller
    {
        private readonly AppDbContext _context;

        public AdminProductsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new AdminProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    AvailableQuantity = p.AvailableQuantity,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.category.Name,
                    SellerName = p.Seller.user != null ? p.Seller.user.Name : "-",
                    ShopName = p.Seller.ShopName,
                    ReviewsCount = p.reviews.Count(),
                    OrdersCount = p.orderItems.Count()
                })
                .ToListAsync();

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["AdminError"] = "Invalid product id.";
                return RedirectToAction(nameof(Index));
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                TempData["AdminError"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            // لا نحذف منتجًا موجودًا في طلبات سابقة حتى لا تنكسر سجلات الطلبات
            bool isInOrders = await _context.OrderItems.AnyAsync(oi => oi.productId == id);
            if (isInOrders)
            {
                TempData["AdminError"] =
                    $"\"{product.Name}\" cannot be removed because it exists in previous orders.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var cartItems = await _context.CartIItems.Where(ci => ci.productId == id).ToListAsync();
                var wishListItems = await _context.WishListItems.Where(wi => wi.ProductId == id).ToListAsync();
                var reviews = await _context.Reviews.Where(r => r.ProductId == id).ToListAsync();

                _context.CartIItems.RemoveRange(cartItems);
                _context.WishListItems.RemoveRange(wishListItems);
                _context.Reviews.RemoveRange(reviews);
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["AdminSuccess"] = $"\"{product.Name}\" has been removed.";
            }
            catch (DbUpdateException)
            {
                TempData["AdminError"] = "The product could not be removed. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}