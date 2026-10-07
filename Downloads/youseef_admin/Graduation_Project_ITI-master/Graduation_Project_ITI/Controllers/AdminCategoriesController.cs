using BLL.Configuration;
using DAL;
using Graduation_Project_ITI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project_ITI.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminCategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public AdminCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categorys
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new AdminCategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Comment = c.Comment,
                    CreatedAt = c.CreateAt,
                    ProductsCount = c.products.Count()
                })
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AdminCategoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCategoryFormViewModel model)
        {
            var name = (model.Name ?? string.Empty).Trim();

            if (ModelState.IsValid && await NameExistsAsync(name, null))
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Categorys.Add(new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                Comment = (model.Comment ?? string.Empty).Trim(),
                CreateAt = DateTime.Now
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "The category could not be saved. Please try again.");
                return View(model);
            }

            TempData["AdminSuccess"] = $"Category \"{name}\" has been added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["AdminError"] = "Invalid category id.";
                return RedirectToAction(nameof(Index));
            }

            var category = await _context.Categorys.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                TempData["AdminError"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(new AdminCategoryFormViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Comment = category.Comment
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminCategoryFormViewModel model)
        {
            if (model.Id == Guid.Empty)
            {
                TempData["AdminError"] = "Invalid category id.";
                return RedirectToAction(nameof(Index));
            }

            var category = await _context.Categorys.FirstOrDefaultAsync(c => c.Id == model.Id);
            if (category == null)
            {
                TempData["AdminError"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            var name = (model.Name ?? string.Empty).Trim();

            if (ModelState.IsValid && await NameExistsAsync(name, model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            category.Name = name;
            category.Comment = (model.Comment ?? string.Empty).Trim();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "The category could not be saved. Please try again.");
                return View(model);
            }

            TempData["AdminSuccess"] = $"Category \"{name}\" has been updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["AdminError"] = "Invalid category id.";
                return RedirectToAction(nameof(Index));
            }

            var category = await _context.Categorys.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                TempData["AdminError"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            int productsCount = await _context.Products.CountAsync(p => p.categoryId == id);
            if (productsCount > 0)
            {
                TempData["AdminError"] =
                    $"Category \"{category.Name}\" cannot be deleted because it has {productsCount} product(s).";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Categorys.Remove(category);
                await _context.SaveChangesAsync();
                TempData["AdminSuccess"] = $"Category \"{category.Name}\" has been deleted.";
            }
            catch (DbUpdateException)
            {
                TempData["AdminError"] = "The category could not be deleted. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // فحص تكرار الاسم بدون حساسية لحالة الأحرف. excludeId يُستثنى عند التعديل.
        private async Task<bool> NameExistsAsync(string name, Guid? excludeId)
        {
            var lowered = name.ToLower();

            return await _context.Categorys.AnyAsync(c =>
                c.Name.ToLower() == lowered && (excludeId == null || c.Id != excludeId));
        }
    }
}