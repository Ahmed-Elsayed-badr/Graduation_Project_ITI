using Microsoft.AspNetCore.Mvc;
using Graduation_Project_ITI.Models;
using BLL.Configuration;
using System;
using System.Linq;
using System.Security.Claims;

namespace Graduation_Project_ITI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchTerm, Guid? categoryId, string sortOrder)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.categoryId == categoryId.Value);
            }

            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            var products = query
                .Select(p => new ProductListViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.category.Name,
                    ImageUrl = p.ImageUrl,
                    AvailableQuantity = p.AvailableQuantity
                })
                .ToList();

            ViewBag.Categories = _context.Categorys.ToList();
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSort = sortOrder;

            return View(products);
        }

        public IActionResult Details(Guid id)
        {
            var product = _context.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductDetailsViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    AvailableQuantity = p.AvailableQuantity,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.category.Name,
                    SellerName = p.Seller.user.Name,
                    AverageRating = p.reviews.Any() ? p.reviews.Average(r => r.Rating) : 0,
                    ReviewsCount = p.reviews.Count(),
                    Reviews = p.reviews
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => new ReviewViewModel
                        {
                            ReviewerName = r.user.Name,
                            Rating = r.Rating,
                            Comment = r.Comment,
                            CreatedAt = r.CreatedAt
                        }).ToList()
                })
                .FirstOrDefault();

            if (product == null)
            {
                return NotFound();
            }

            // Determine if the logged-in user can leave a review:
            // they must have purchased this product and not have reviewed it already
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                bool purchased = _context.OrderItems
                    .Any(oi => oi.productId == id && oi.order.userId == userId);

                bool alreadyReviewed = _context.Reviews
                    .Any(r => r.ProductId == id && r.userId == userId);

                product.CanReview = purchased && !alreadyReviewed;
            }

            return View(product);
        }
    }
}