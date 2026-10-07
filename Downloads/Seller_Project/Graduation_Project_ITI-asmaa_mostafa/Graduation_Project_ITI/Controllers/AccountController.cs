
using BLL.Configuration;
using DAL;
using Graduation_Project_ITI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Graduation_Project_ITI.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(
            AppDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // =========================================================
        // REGISTER - GET
        // =========================================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        // =========================================================
        // REGISTER - POST
        // =========================================================

       
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Shop name is required only for Seller
            if (model.Role == "Seller" &&
                string.IsNullOrWhiteSpace(model.ShopName))
            {
                ModelState.AddModelError(
                    nameof(model.ShopName),
                    "Shop name is required for sellers.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered.");

                return View(model);
            }

            try
            {
                // Create Wishlist
                var wishList = new WishList
                {
                    Id = Guid.NewGuid()
                };

                // Create User
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Email = model.Email,
                    phonenumber = model.PhoneNumber,
                    IsActive = 1,
                    Role = model.Role,
                    WishList = wishList
                };

                // Hash password
                user.PasswordHash =
                    _passwordHasher.HashPassword(
                        user,
                        model.Password);

                // Add Wishlist
                _context.WishLists.Add(wishList);

                // Add User
                _context.Users.Add(user);

                // If user registered as Seller
                if (model.Role == "Seller")
                {
                    var seller = new Seller
                    {
                        Id = Guid.NewGuid(),
                        userId = user.Id,
                        ShopName = model.ShopName!.Trim(),
                        Status = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Sellers.Add(seller);
                }

                // Save User + Wishlist + Seller
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Login));
            }
            catch (DbUpdateException ex)
            {
                var errorMessage =
                    ex.InnerException?.Message
                    ?? ex.Message;

                ModelState.AddModelError(
                    "",
                    $"Database error: {errorMessage}");

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    $"Registration error: {ex.Message}");

                return View(model);
            }
        }



        // =========================================================
        // LOGIN - GET
        // =========================================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        // =========================================================
        // LOGIN - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(model);
            }

            var result =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(model);
            }

            // Check account status
            if (user.IsActive != 1)
            {
                ModelState.AddModelError(
                    "",
                    "The account is not active.");

                return View(model);
            }

            // =====================================================
            // AUTHENTICATION CLAIMS
            // =====================================================

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Name),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                "CookieAuth");

            var claimsPrincipal =
                new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(
                "CookieAuth",
                claimsPrincipal);

            // =====================================================
            // REDIRECT BASED ON ROLE
            // =====================================================

            if (user.Role == "Administrator")
            {
                return RedirectToAction(
                    "Index",
                    "Admin");
            }

            if (user.Role == "Seller")
            {
                return RedirectToAction(
                    "Index",
                    "Products");
            }

            // Customer
            return RedirectToAction(
                "Index",
                "Home");
        }

        //public async Task<IActionResult> Shope(User user)
        //{

        //}


        // =========================================================
        // LOGOUT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");

            return RedirectToAction(nameof(Login));
        }
    }
}

