using Microsoft.AspNetCore.Mvc;
using Graduation_Project_ITI.Models;
using BLL.Configuration;
using System;
using System.Linq;

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
                    ReviewsCount = p.reviews.Count()
                })
                .FirstOrDefault();

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}