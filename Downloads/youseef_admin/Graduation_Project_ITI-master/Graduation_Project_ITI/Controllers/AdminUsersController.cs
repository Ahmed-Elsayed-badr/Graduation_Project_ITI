using BLL.Configuration;
using Graduation_Project_ITI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Graduation_Project_ITI.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminUsersController : Controller
    {
        private readonly AppDbContext _context;

        public AdminUsersController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId);

            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Name)
                .Select(u => new AdminUserViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    PhoneNumber = u.phonenumber,
                    Role = u.Role,
                    IsActive = u.IsActive == 1,
                    CreatedAt = u.DateTime,
                    IsAdministrator = u.Role == "Administrator",
                    IsCurrentUser = u.Id == currentUserId
                })
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(Guid id)
        {
            var user = await FindUserAsync(id);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (user.IsActive == 1)
            {
                TempData["AdminError"] = $"{user.Name} is already active.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = 1;
            return await SaveAsync($"{user.Name} has been activated.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(Guid id)
        {
            var user = await FindUserAsync(id);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId);

            if (user.Id == currentUserId)
            {
                TempData["AdminError"] = "You cannot suspend your own account.";
                return RedirectToAction(nameof(Index));
            }

            if (user.Role == "Administrator")
            {
                TempData["AdminError"] = "Administrator accounts cannot be suspended.";
                return RedirectToAction(nameof(Index));
            }

            if (user.IsActive != 1)
            {
                TempData["AdminError"] = $"{user.Name} is already suspended.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = 0;
            return await SaveAsync($"{user.Name} has been suspended.");
        }

        private async Task<DAL.User?> FindUserAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["AdminError"] = "Invalid user id.";
                return null;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["AdminError"] = "User not found.";
            }

            return user;
        }

        private async Task<IActionResult> SaveAsync(string successMessage)
        {
            try
            {
                await _context.SaveChangesAsync();
                TempData["AdminSuccess"] = successMessage;
            }
            catch (DbUpdateException)
            {
                TempData["AdminError"] = "The change could not be saved. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}