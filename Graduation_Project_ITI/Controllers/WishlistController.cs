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
    public class WishlistController : Controller
    {
        private readonly AppDbContext _context;

        public WishlistController(AppDbContext context)
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

            // Every user already has a WishList created at registration
            var wishList = _context.WishLists.FirstOrDefault(w => w.UserId == userId);

            var items = wishList == null
                ? new List<WishlistItemViewModel>()
                : _context.WishListItems
                    .Where(wi => wi.WishListId == wishList.Id)
                    .Select(wi => new WishlistItemViewModel
                    {
                        WishListItemId = wi.Id,
                        ProductId = wi.ProductId,
                        ProductName = wi.Product.Name,
                        ImageUrl = wi.Product.ImageUrl,
                        Price = wi.Product.Price,
                        AvailableQuantity = wi.Product.AvailableQuantity
                    })
                    .ToList();

            return View(items);
        }

        [HttpPost]
        public IActionResult AddToWishlist(Guid productId)
        {
            var userId = GetUserId();

            var wishList = _context.WishLists.FirstOrDefault(w => w.UserId == userId);
            if (wishList == null)
            {
                // Shouldn't normally happen since a WishList is created at registration
                wishList = new WishList { Id = Guid.NewGuid(), UserId = userId };
                _context.WishLists.Add(wishList);
                _context.SaveChanges();
            }

            bool alreadyAdded = _context.WishListItems
                .Any(wi => wi.WishListId == wishList.Id && wi.ProductId == productId);

            if (!alreadyAdded)
            {
                _context.WishListItems.Add(new WishListItem
                {
                    Id = Guid.NewGuid(),
                    WishListId = wishList.Id,
                    ProductId = productId
                });
                _context.SaveChanges();
            }

            return RedirectToAction("Details", "Products", new { id = productId });
        }

        [HttpPost]
        public IActionResult RemoveFromWishlist(Guid wishListItemId)
        {
            var item = _context.WishListItems.FirstOrDefault(wi => wi.Id == wishListItemId);
            if (item != null)
            {
                _context.WishListItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}