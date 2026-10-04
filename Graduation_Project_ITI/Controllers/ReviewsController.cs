using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.Configuration;
using DAL;
using System;
using System.Linq;
using System.Security.Claims;

namespace Graduation_Project_ITI.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(idClaim);
        }

        [HttpPost]
        public IActionResult Add(Guid productId, int rating, string comment)
        {
            var userId = GetUserId();

            if (rating < 1 || rating > 5)
            {
                TempData["ReviewError"] = "Rating must be between 1 and 5.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // Re-check server-side: must have purchased the product
            bool purchased = _context.OrderItems
                .Any(oi => oi.productId == productId && oi.order.userId == userId);

            if (!purchased)
            {
                TempData["ReviewError"] = "You can only review products you have purchased.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            bool alreadyReviewed = _context.Reviews
                .Any(r => r.ProductId == productId && r.userId == userId);

            if (alreadyReviewed)
            {
                TempData["ReviewError"] = "You already reviewed this product.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            _context.Reviews.Add(new Reviews
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                userId = userId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            });

            _context.SaveChanges();

            return RedirectToAction("Details", "Products", new { id = productId });
        }
    }
}