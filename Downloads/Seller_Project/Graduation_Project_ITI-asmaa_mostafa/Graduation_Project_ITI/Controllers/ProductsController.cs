
using BLL.Configuration;
using BLL.ReviewInter;
using BLL.Services.Interfaces;
using DAL;
using Graduation_Project_ITI.Interfaces;
using Graduation_Project_ITI.Models;
using Graduation_Project_ITI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Graduation_Project_ITI.Controllers
{
    public class ProductsController : Controller
    {
        private const string ImagesRequestPath = "/images/products/";
        private const long MaxImageSizeBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp"
        };

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ISellerOrderInterface _sellerOrderService;
        private readonly IReviewInterface _reviewService;
        private readonly AppDbContext _context;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context,
            ISellerOrderInterface sellerOrderService,
            IReviewInterface reviewService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
            _sellerOrderService = sellerOrderService;
            _reviewService = reviewService;
            _context = context;
        }

        // ============================================================
        // PUBLIC PRODUCTS
        // ============================================================

        // GET: /Products
        // Public catalog - does not depend on logged-in user
       
[Authorize(Roles = "Seller")]
[HttpGet]
    public async Task<IActionResult> Index(
    string? searchTerm,
    Guid? categoryId,
    bool? sortAscending)
        {
            // 1. Get the User.Id of the logged-in user
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            // 2. Get the Seller profile belonging to this user
            var seller = await _context.Sellers
                .FirstOrDefaultAsync(s => s.userId == userId);

            if (seller == null)
            {
                return NotFound("Seller profile not found.");
            }

            // 3. Get ONLY this seller's products
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.SellerId == seller.Id)
                .ToListAsync();

            // 4. Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products
                    .Where(p => p.Name.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // 5. Filter by category
            if (categoryId.HasValue)
            {
                products = products
                    .Where(p => p.CategoryId == categoryId.Value)
                    .ToList();
            }

            // 6. Sort by price
            if (sortAscending.HasValue)
            {
                products = sortAscending.Value
                    ? products.OrderBy(p => p.Price).ToList()
                    : products.OrderByDescending(p => p.Price).ToList();
            }

            // 7. Get categories
            var categories =
                await _categoryService.GetAllCategoriesAsync();

            ViewBag.Categories = new SelectList(
                categories,
                "Id",
                "Name",
                categoryId);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortAscending = sortAscending;

            // 8. Return ONLY the current seller's products
            return View(products);
        }




        // ============================================================
        // PRODUCT DETAILS
        // ============================================================

        // GET: /Products/Details/{id}
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }


        // ============================================================
        // CREATE PRODUCT
        // ONLY LOGGED-IN SELLER
        // ============================================================

        // GET: /Products/Create
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
                return Forbid();

            var vm = new ProductViewModel
            {
                Categories = await GetCategorySelectListAsync()
            };

            return View(vm);
        }


        // POST: /Products/Create
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel vm)
        {
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
                return Forbid();

            if (vm.ImageFile == null)
            {
                ModelState.AddModelError(
                    nameof(vm.ImageFile),
                    "Product image is required.");
            }

            if (!ModelState.IsValid)
            {
                vm.Categories = await GetCategorySelectListAsync();
                return View(vm);
            }

            string? imageUrl = null;

            try
            {
                imageUrl = await SaveImageAsync(vm.ImageFile!);

                // IMPORTANT:
                // SellerId comes from the logged-in user.
                // We DO NOT trust vm.SellerId.
                var product = new Product
                {
                    Name = vm.Name,
                    Description = vm.Description,
                    Price = vm.Price,
                    AvailableQuantity = vm.AvailableQuantity,
                    ImageUrl = imageUrl,
                    CategoryId = vm.CategoryId,
                    SellerId = seller.Id
                };

                await _productService.CreateProductAsync(product);

                TempData["Success"] =
                    $"Product \"{product.Name}\" was created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                DeleteImageFile(imageUrl);

                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);
            }
            catch (DbUpdateException)
            {
                DeleteImageFile(imageUrl);

                ModelState.AddModelError(
                    string.Empty,
                    "The product could not be saved. Please check the data and try again.");
            }

            vm.Categories = await GetCategorySelectListAsync();

            return View(vm);
        }


        // ============================================================
        // EDIT PRODUCT
        // ONLY THE OWNER SELLER
        // ============================================================

        // GET: /Products/Edit/{id}
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
                return Forbid();

            // Get only a product that belongs to the logged-in seller
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.SellerId == seller.Id);

            if (product == null)
                return NotFound();

            var vm = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                AvailableQuantity = product.AvailableQuantity,
                ExistingImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,

                // This is only for displaying existing data.
                // We don't trust it when saving.
                SellerId = product.SellerId,

                Categories = await GetCategorySelectListAsync()
            };

            return View(vm);
        }


        // POST: /Products/Edit
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel vm)
        {
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
                return Forbid();

            // IMPORTANT:
            // Check ownership before modifying anything.
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == vm.Id &&
                    p.SellerId == seller.Id);

            if (product == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                vm.Categories = await GetCategorySelectListAsync();
                return View(vm);
            }

            string? newImageUrl = null;

            try
            {
                var oldImageUrl = product.ImageUrl;

                if (vm.ImageFile != null)
                {
                    newImageUrl = await SaveImageAsync(vm.ImageFile);

                    product.ImageUrl = newImageUrl;
                }

                // Update ONLY the fields allowed from the form.
                product.Name = vm.Name;
                product.Description = vm.Description;
                product.Price = vm.Price;
                product.AvailableQuantity = vm.AvailableQuantity;
                product.CategoryId = vm.CategoryId;

                // VERY IMPORTANT:
                // We don't change product.SellerId.
                // It remains the logged-in seller's ID.

                await _context.SaveChangesAsync();

                if (newImageUrl != null)
                {
                    DeleteImageFile(oldImageUrl);
                }

                TempData["Success"] =
                    $"Product \"{product.Name}\" was updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                DeleteImageFile(newImageUrl);

                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);
            }
            catch (DbUpdateException)
            {
                DeleteImageFile(newImageUrl);

                ModelState.AddModelError(
                    string.Empty,
                    "The product could not be updated. Please check the data and try again.");
            }

            vm.Categories = await GetCategorySelectListAsync();

            return View(vm);
        }


        // ============================================================
        // DELETE PRODUCT
        // ONLY THE OWNER SELLER
        // ============================================================

        // GET: /Products/Delete/{id}
        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
                return Forbid();

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.SellerId == seller.Id);

            if (product == null)
                return NotFound();

            return View(product);
        }


        // POST: /Products/Delete/{id}
        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
                return Forbid();

            // Make sure this product belongs to the logged-in seller.
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.SellerId == seller.Id);

            if (product == null)
                return NotFound();

            var imageUrl = product.ImageUrl;
            var name = product.Name;

            try
            {
                await _productService.DeleteProductAsync(id);
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    $"\"{name}\" could not be deleted because it is used by other records (orders, carts...).";

                return RedirectToAction(nameof(Index));
            }

            DeleteImageFile(imageUrl);

            TempData["Success"] =
                $"Product \"{name}\" was deleted successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // SELLER ORDERS
        // ONLY LOGGED-IN SELLER
        // ============================================================

        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
                return Forbid();

            // Orders are loaded using the CURRENT SELLER.
            var orders = await _sellerOrderService
                .GetSellerOrders(seller.Id);

            return View(orders);
        }


        // ============================================================
        // ORDER DETAILS
        // ============================================================

        //[Authorize]
        //[HttpGet]
        //public async Task<IActionResult> OrderDetails(Guid id)
        //{
        //    var product = await _context.Products
        //        .Include(p => p.Category)
        //        .FirstOrDefaultAsync(p => p.Id == id);

        //    if (product == null)
        //        return NotFound();

        //    var summary = new ProductReviewsSummaryVM
        //    {
        //        Reviews = await _reviewService.GetProductReviews(id),
        //        AverageRating = await _reviewService.GetAverageRating(id)
        //    };

        //    summary.ReviewsCount = summary.Reviews.Count;

        //    // ========================================================
        //    // CURRENT LOGGED-IN USER
        //    // ========================================================

        //    var currentUserId = GetCurrentUserId();

        //    if (currentUserId.HasValue)
        //    {
        //        var purchased =
        //            await _reviewService.HasUserPurchasedProduct(
        //                currentUserId.Value,
        //                id);

        //        var alreadyReviewed =
        //            await _reviewService.HasUserAlreadyReviewed(
        //                currentUserId.Value,
        //                id);

        //        summary.CanReview =
        //            purchased && !alreadyReviewed;
        //    }

        //    ViewBag.ReviewsSummary = summary;

        //    return View(product);
        //}
        

[Authorize(Roles = "Seller")]
[HttpGet]
public async Task<IActionResult> OrderDetails(Guid id)
        {
            // ============================================
            // 1. Get current logged-in user
            // ============================================

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }


            // ============================================
            // 2. Get current seller
            // ============================================

            var seller = await _context.Sellers
                .FirstOrDefaultAsync(s => s.userId == userId);

            if (seller == null)
            {
                return NotFound("Seller profile not found.");
            }


            // ============================================
            // 3. Get order
            // ============================================

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.product)
                .Include(o => o.user)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Order not found.");
            }


            // ============================================
            // 4. Get only this seller's order items
            // ============================================

            var sellerItems = order.OrderItems
                .Where(oi =>
                    oi.product != null &&
                    oi.product.SellerId == seller.Id)
                .ToList();

            if (!sellerItems.Any())
            {
                return NotFound("This order does not belong to you.");
            }


            // ============================================
            // 5. Build SellerOrderDetailsVM
            // ============================================

            var model = new SellerOrderDetailsVM
            {
                OrderId = order.Id,

                OrderDate = DateTime.UtcNow,

                Status = order.Status,

                CustomerName = order.user?.Name,

                CustomerEmail = order.user?.Email,

                Items = sellerItems
                    .Select(oi => new SellerOrderItemVM
                    {
                        ProductId = oi.productId,

                        ProductName = oi.product.Name,

                        Quantity = oi.Quantity,

                        UnitPrice = oi.UnitPrice,
                        OrderItemId = oi.Id,
                        Status = oi.Status,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    })
                    .ToList()
            };


            // ============================================
            // 6. Return ViewModel
            // ============================================

            return View(model);
        }




        // ============================================================
        // UPDATE ORDER ITEM STATUS
        // ONLY THE SELLER WHO OWNS THE PRODUCT
        // ============================================================


        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderItemStatus(
    Guid orderItemId,
    string newStatus,
    Guid orderId)
        {
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
            {
                return Unauthorized();
            }

            var item = await _context.OrderItems
                .Include(x => x.order)
                .Include(x => x.product)
                .FirstOrDefaultAsync(x => x.Id == orderItemId);

            if (item == null)
            {
                return NotFound();
            }

            if (item.product == null)
            {
                return NotFound();
            }

            if (item.product.SellerId != seller.Id)
            {
                return Forbid();
            }

            item.Status = newStatus;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(OrderDetails),
                new { id = orderId }
            );
        }




        // ============================================================
        // GET CURRENT USER ID
        // ============================================================

        private Guid? GetCurrentUserId()
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }

            return null;
        }


        // ============================================================
        // GET CURRENT SELLER
        // ============================================================

        private async Task<Seller?> GetCurrentSellerAsync()
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return null;
            }

            return await _context.Sellers
                .FirstOrDefaultAsync(s =>
                    s.userId == userId.Value);
        }


        // ============================================================
        // SAVE IMAGE
        // ============================================================

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile.Length == 0)
            {
                throw new ArgumentException(
                    "The selected image file is empty.");
            }

            if (imageFile.Length > MaxImageSizeBytes)
            {
                throw new ArgumentException(
                    "The image is too large. Maximum allowed size is 5 MB.");
            }

            var extension =
                Path.GetExtension(imageFile.FileName)
                    .ToLowerInvariant();

            if (!AllowedImageExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    "Invalid image type. Allowed types: JPG, PNG, GIF, WEBP.");
            }

            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "images",
                "products");

            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName =
                Guid.NewGuid().ToString("N") + extension;

            var filePath =
                Path.Combine(
                    uploadsFolder,
                    uniqueFileName);

            await using (var fileStream =
                new FileStream(
                    filePath,
                    FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return ImagesRequestPath + uniqueFileName;
        }
         

      

        // ============================================================
        // DELETE IMAGE
        // ============================================================

        private void DeleteImageFile(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) ||
                !imageUrl.StartsWith(
                    ImagesRequestPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var fileName =
                Path.GetFileName(imageUrl);

            var filePath =
                Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "images",
                    "products",
                    fileName);

            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (IOException)
            {
                // Do not break the request if image deletion fails.
            }
        }


        // ============================================================
        // CATEGORY SELECT LIST
        // ============================================================

        private async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            var categories =
                await _categoryService.GetAllCategoriesAsync();

            return categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();
        }


   
[Authorize(Roles = "Seller")]
[HttpGet]
public async Task<IActionResult> Analysis()
        {
            // ============================================
            // 1. Get Current Seller
            // ============================================

            var seller = await GetCurrentSellerAsync();

            if (seller == null)
            {
                return Unauthorized();
            }


            // ============================================
            // 2. Total Products
            // ============================================

            var totalProducts = await _context.Products
                .CountAsync(p => p.SellerId == seller.Id);


            // ============================================
            // 3. Get Seller Order Items
            // ============================================

            var orderItems = await _context.OrderItems
                .AsNoTracking()
                .Include(oi => oi.order)
                    .ThenInclude(o => o.user)
                .Include(oi => oi.product)
                    .ThenInclude(p => p.Category)
                .Where(oi => oi.SellerId == seller.Id)
                .ToListAsync();


            // ============================================
            // 4. Valid Items
            // ============================================

            var validItems = orderItems
                .Where(oi =>
                    !string.Equals(
                        oi.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();


            // ============================================
            // 5. TOTAL SALES
            // ============================================

            var totalSales = validItems.Sum(oi =>
                oi.Quantity * oi.UnitPrice);


            // ============================================
            // 6. TOTAL ORDERS
            // ============================================

            var totalOrders = orderItems
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();


            // ============================================
            // 7. ORDER STATUS
            // ============================================

            var completedOrders = orderItems
                .Where(oi =>
                    string.Equals(
                        oi.Status,
                        "Delivered",
                        StringComparison.OrdinalIgnoreCase))
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();


            var pendingOrders = orderItems
                .Where(oi =>
                    string.Equals(
                        oi.Status,
                        "Pending",
                        StringComparison.OrdinalIgnoreCase))
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();


            var confirmedOrders = orderItems
                .Where(oi =>
                    string.Equals(
                        oi.Status,
                        "Confirmed",
                        StringComparison.OrdinalIgnoreCase))
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();


            var shippedOrders = orderItems
                .Where(oi =>
                    string.Equals(
                        oi.Status,
                        "Shipped",
                        StringComparison.OrdinalIgnoreCase))
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();


            var cancelledOrders = orderItems
                .Where(oi =>
                    string.Equals(
                        oi.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase))
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();


            // ============================================
            // 8. PRODUCTS SOLD
            // ============================================

            var totalProductsSold = validItems.Sum(oi =>
                oi.Quantity);


            // ============================================
            // 9. AVERAGE ORDER VALUE
            // ============================================

            var averageOrderValue = totalOrders > 0
                ? totalSales / totalOrders
                : 0;


            // ============================================
            // 10. TOP 3 PRODUCTS
            // ============================================

            var topProducts = validItems
                .GroupBy(oi => new
                {
                    oi.productId,
                    ProductName = oi.product.Name,
                    ImageUrl = oi.product.ImageUrl
                })
                .Select(g => new SellerAnalysisProductVM
                {
                    ProductId = g.Key.productId,

                    ProductName = g.Key.ProductName,

                    ImageUrl = g.Key.ImageUrl,

                    QuantitySold = g.Sum(x => x.Quantity),

                    Sales = g.Sum(x =>
                        x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.QuantitySold)
                .ThenByDescending(x => x.Sales)
                .Take(3)
                .ToList();


            // ============================================
            // 11. SALES BY CATEGORY
            // ============================================

            var categorySales = validItems
                .GroupBy(oi =>
                    oi.product.Category != null
                        ? oi.product.Category.Name
                        : "Uncategorized")
                .Select(g => new SellerAnalysisCategoryVM
                {
                    CategoryName = g.Key,

                    QuantitySold = g.Sum(x => x.Quantity),

                    Sales = g.Sum(x =>
                        x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.Sales)
                .ToList();


            // ============================================
            // 12. RECENT ORDERS
            // ============================================

            var recentOrders = orderItems
                .GroupBy(oi => oi.OrderId)
                .Select(g =>
                {
                    var firstItem = g.First();

                    return new SellerAnalysisOrderVM
                    {
                        OrderId = firstItem.OrderId,

                        OrderDate = firstItem.order.OrderDate,

                        CustomerName =
                            firstItem.order.user != null
                                ? firstItem.order.user.Name
                                : "Unknown Customer",

                        Status = GetOrderAnalysisStatus(g.ToList()),

                        Total = g
                            .Where(x =>
                                !string.Equals(
                                    x.Status,
                                    "Cancelled",
                                    StringComparison.OrdinalIgnoreCase))
                            .Sum(x =>
                                x.Quantity * x.UnitPrice),

                        ItemsCount = g.Sum(x => x.Quantity)
                    };
                })
                .OrderByDescending(x => x.OrderDate)
                .Take(8)
                .ToList();


            // ============================================
            // 13. SALES & ORDERS TREND
            // Last 30 Days
            // ============================================

            var today = DateTime.Today;

            var currentPeriodStart = today.AddDays(-29);

            var previousPeriodStart = today.AddDays(-59);

            var previousPeriodEnd = currentPeriodStart.AddDays(-1);


            // --------------------------------------------
            // Current 30 Days
            // --------------------------------------------

            var currentPeriodItems = orderItems
                .Where(oi =>
                    oi.order.OrderDate.Date >= currentPeriodStart &&
                    oi.order.OrderDate.Date <= today)
                .ToList();


            // --------------------------------------------
            // Previous 30 Days
            // --------------------------------------------

            var previousPeriodItems = orderItems
                .Where(oi =>
                    oi.order.OrderDate.Date >= previousPeriodStart &&
                    oi.order.OrderDate.Date <= previousPeriodEnd)
                .ToList();


            // ============================================
            // Sales Growth
            // ============================================

            var currentSales = currentPeriodItems
                .Where(oi =>
                    !string.Equals(
                        oi.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase))
                .Sum(oi =>
                    oi.Quantity * oi.UnitPrice);


            var previousSales = previousPeriodItems
                .Where(oi =>
                    !string.Equals(
                        oi.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase))
                .Sum(oi =>
                    oi.Quantity * oi.UnitPrice);


            decimal salesGrowth = 0;

            if (previousSales > 0)
            {
                salesGrowth =
                    ((currentSales - previousSales)
                    / previousSales) * 100;
            }


            // ============================================
            // Orders Growth
            // ============================================

            var currentOrders = currentPeriodItems
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();


            var previousOrders = previousPeriodItems
                .Select(oi => oi.OrderId)
                .Distinct()
                .Count();


            decimal ordersGrowth = 0;

            if (previousOrders > 0)
            {
                ordersGrowth =
                    ((decimal)(currentOrders - previousOrders)
                    / previousOrders) * 100;
            }


            // ============================================
            // 14. TREND DATA
            // ============================================

            var trendLabels = new List<string>();

            var salesTrend = new List<decimal>();

            var ordersTrend = new List<int>();


            for (int i = 0; i < 30; i++)
            {
                var date = currentPeriodStart.AddDays(i);

                trendLabels.Add(date.ToString("MMM dd"));


                // -----------------------------
                // Daily Sales
                // -----------------------------

                var dailySales = currentPeriodItems
                    .Where(oi =>
                        oi.order.OrderDate.Date == date &&
                        !string.Equals(
                            oi.Status,
                            "Cancelled",
                            StringComparison.OrdinalIgnoreCase))
                    .Sum(oi =>
                        oi.Quantity * oi.UnitPrice);


                salesTrend.Add(dailySales);


                // -----------------------------
                // Daily Orders
                // -----------------------------

                var dailyOrders = currentPeriodItems
                    .Where(oi =>
                        oi.order.OrderDate.Date == date)
                    .Select(oi => oi.OrderId)
                    .Distinct()
                    .Count();


                ordersTrend.Add(dailyOrders);
            }


            // ============================================
            // 15. Build ViewModel
            // ============================================

            var model = new SellerAnalysisVM
            {
                SellerName = seller.ShopName,

                TotalProducts = totalProducts,

                TotalSales = totalSales,

                TotalOrders = totalOrders,

                TotalProductsSold = totalProductsSold,

                AverageOrderValue = averageOrderValue,

                CompletedOrders = completedOrders,

                PendingOrders = pendingOrders,

                ConfirmedOrders = confirmedOrders,

                ShippedOrders = shippedOrders,

                CancelledOrders = cancelledOrders,

                SalesGrowthPercentage = salesGrowth,

                OrdersGrowthPercentage = ordersGrowth,

                TrendLabels = trendLabels,

                SalesTrend = salesTrend,

                OrdersTrend = ordersTrend,

                TopProducts = topProducts,

                CategorySales = categorySales,

                RecentOrders = recentOrders
            };


            // ============================================
            // 16. Return View
            // ============================================

            return View(model);
        }



        private string GetOrderAnalysisStatus(List<OrderItem> items)
        {
            if (items == null || !items.Any())
            {
                return "Unknown";
            }

            var statuses = items
                .Select(x => x.Status?.Trim().ToLower())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();


            // ============================================
            // Cancelled
            // ============================================

            if (statuses.All(x => x == "cancelled"))
            {
                return "Cancelled";
            }


            // ============================================
            // Delivered
            // ============================================

            if (statuses.All(x => x == "delivered"))
            {
                return "Delivered";
            }


            // ============================================
            // Shipped
            // ============================================

            if (statuses.Any(x => x == "shipped"))
            {
                return "Shipped";
            }


            // ============================================
            // Confirmed
            // ============================================

            if (statuses.Any(x => x == "confirmed"))
            {
                return "Confirmed";
            }


            // ============================================
            // Pending
            // ============================================

            if (statuses.Any(x => x == "pending"))
            {
                return "Pending";
            }


            return "Unknown";
        }

        [Authorize(Roles = "Seller")]
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == seller.userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new SellerSettingsVM
            {
                SellerId = seller.Id,
                UserId = user.Id,

                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.phonenumber,

                ShopName = seller.ShopName,
                StoreStatus = seller.Status,
                IsApproved = seller.IsApproved,
                CreatedAt = seller.CreatedAt
            };

            return View(model);
        }


        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(SellerSettingsVM model)
        {
            // ============================================
            // 1. Get current seller
            // ============================================
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
            {
                return Unauthorized();
            }

            // ============================================
            // 2. Get user using Seller.userId
            // ============================================
            if (!seller.userId.HasValue)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == seller.userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            // ============================================
            // 3. Validate model
            // ============================================
            if (!ModelState.IsValid)
            {
                // Keep seller data
                model.SellerId = seller.Id;
                model.UserId = user.Id;
                model.ShopName = seller.ShopName;
                model.StoreStatus = seller.Status;
                model.IsApproved = seller.IsApproved;
                model.CreatedAt = seller.CreatedAt;

                return View("Settings", model);
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.phonenumber = model.PhoneNumber;

         
            await _context.SaveChangesAsync();

        
            TempData["Success"] = "Profile information updated successfully.";

            return RedirectToAction(nameof(Settings));
        }

        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStore(SellerSettingsVM model)
        {
      
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
            {
                return Unauthorized();
            }

        
            if (string.IsNullOrWhiteSpace(model.ShopName))
            {
                TempData["Error"] = "Shop name is required.";
                return RedirectToAction(nameof(Settings));
            }

            var shopName = model.ShopName.Trim();

            if (shopName.Length < 5)
            {
                TempData["Error"] = "Shop name must be at least 5 characters.";
                return RedirectToAction(nameof(Settings));
            }

            if (shopName.Length > 80)
            {
                TempData["Error"] = "Shop name must be less than or equal to 80 characters.";
                return RedirectToAction(nameof(Settings));
            }

      
            seller.ShopName = shopName;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Store information updated successfully.";

            return RedirectToAction(nameof(Settings));
        }

        [Authorize(Roles = "Seller")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(SellerSettingsVM model)
        {
            var seller = await GetCurrentSellerAsync();

            if (seller == null)
            {
                return Unauthorized();
            }

            var user = seller.user;

            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                TempData["Error"] = "Please fill in all password fields.";

                return RedirectToAction(nameof(Settings));
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation password do not match.";

                return RedirectToAction(nameof(Settings));
            }

            // IMPORTANT:
            // Replace this with your existing password verification method.

            if (user.PasswordHash != model.CurrentPassword)
            {
                TempData["Error"] = "Current password is incorrect.";

                return RedirectToAction(nameof(Settings));
            }

            // IMPORTANT:
            // Replace this with your existing password hashing method.

            user.PasswordHash = model.NewPassword;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Password changed successfully.";

            return RedirectToAction(nameof(Settings));
        }
    }
}

