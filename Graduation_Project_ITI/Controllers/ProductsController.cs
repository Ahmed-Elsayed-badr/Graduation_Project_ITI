using BLL.Services.Interfaces;
using DAL;
using Graduation_Project_ITI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project_ITI.Controllers
{
    public class ProductsController : Controller
    {
        private const string ImagesRequestPath = "/images/products/";
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET /Products?searchTerm=&categoryId=&sortAscending=
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchTerm, Guid? categoryId, bool? sortAscending)
        {
            var products = await _productService.GetFilteredProductsAsync(categoryId, searchTerm, sortAscending);
            var categories = await _categoryService.GetAllCategoriesAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortAscending = sortAscending;

            return View(products);
        }

        // GET /Products/Details/{id}
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET /Products/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new ProductViewModel
            {
                Categories = await GetCategorySelectListAsync()
            };
            return View(vm);
        }

        // POST /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel vm)
        {
            if (vm.ImageFile == null)
            {
                ModelState.AddModelError(nameof(vm.ImageFile), "Product image is required.");
            }

            if (ModelState.IsValid)
            {
                string? imageUrl = null;
                try
                {
                    imageUrl = await SaveImageAsync(vm.ImageFile!);

                    var product = new Product
                    {
                        Name = vm.Name,
                        Description = vm.Description,
                        Price = vm.Price,
                        AvailableQuantity = vm.AvailableQuantity,
                        ImageUrl = imageUrl,
                        CategoryId = vm.CategoryId,
                        // Empty until authentication exists; the service then falls back to the default seller.
                        SellerId = vm.SellerId
                    };

                    await _productService.CreateProductAsync(product);
                    TempData["Success"] = $"Product \"{product.Name}\" was created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    DeleteImageFile(imageUrl); // don't leave an orphan file if saving the product failed
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (DbUpdateException)
                {
                    DeleteImageFile(imageUrl);
                    ModelState.AddModelError(string.Empty, "The product could not be saved. Please check the data and try again.");
                }
            }

            vm.Categories = await GetCategorySelectListAsync();
            return View(vm);
        }

        // GET /Products/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var vm = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                AvailableQuantity = product.AvailableQuantity,
                ExistingImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                SellerId = product.SellerId,
                Categories = await GetCategorySelectListAsync()
            };
            return View(vm);
        }

        // POST /Products/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel vm)
        {
            if (ModelState.IsValid)
            {
                string? newImageUrl = null;
                try
                {
                    var imageUrl = vm.ExistingImageUrl ?? string.Empty;
                    if (vm.ImageFile != null)
                    {
                        newImageUrl = await SaveImageAsync(vm.ImageFile);
                        imageUrl = newImageUrl;
                    }

                    var product = new Product
                    {
                        Id = vm.Id,
                        Name = vm.Name,
                        Description = vm.Description,
                        Price = vm.Price,
                        AvailableQuantity = vm.AvailableQuantity,
                        ImageUrl = imageUrl,
                        CategoryId = vm.CategoryId,
                        SellerId = vm.SellerId
                    };

                    await _productService.UpdateProductAsync(product);

                    // The new image is saved, so the old one is no longer needed.
                    if (newImageUrl != null)
                    {
                        DeleteImageFile(vm.ExistingImageUrl);
                    }

                    TempData["Success"] = $"Product \"{product.Name}\" was updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    DeleteImageFile(newImageUrl);
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (DbUpdateException)
                {
                    DeleteImageFile(newImageUrl);
                    ModelState.AddModelError(string.Empty, "The product could not be saved. Please check the data and try again.");
                }
            }

            vm.Categories = await GetCategorySelectListAsync();
            return View(vm);
        }

        // GET /Products/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST /Products/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var imageUrl = product.ImageUrl;
            var name = product.Name;

            try
            {
                await _productService.DeleteProductAsync(id);
            }
            catch (DbUpdateException)
            {
                // e.g. the product is referenced by existing orders
                TempData["Error"] = $"\"{name}\" could not be deleted because it is used by other records (orders, carts...).";
                return RedirectToAction(nameof(Index));
            }

            DeleteImageFile(imageUrl);
            TempData["Success"] = $"Product \"{name}\" was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile.Length == 0)
                throw new ArgumentException("The selected image file is empty.");

            if (imageFile.Length > MaxImageSizeBytes)
                throw new ArgumentException("The image is too large. Maximum allowed size is 5 MB.");

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
                throw new ArgumentException("Invalid image type. Allowed types: JPG, PNG, GIF, WEBP.");

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsFolder);

            // The original file name is never used, so there is no path-traversal risk and no name collisions.
            var uniqueFileName = Guid.NewGuid().ToString("N") + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return ImagesRequestPath + uniqueFileName;
        }

        // Deletes an uploaded product image. Ignores null values, the placeholder and anything outside /images/products/.
        private void DeleteImageFile(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) ||
                !imageUrl.StartsWith(ImagesRequestPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", fileName);

            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (IOException)
            {
                // Not critical: a leftover image file must never break the request.
            }
        }

        private async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }
    }
}
