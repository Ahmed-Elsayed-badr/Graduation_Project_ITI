using BLL.Configuration;
using DAL;
using Graduation_Project_ITI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project_ITI.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminSellerRequestsController : Controller
    {
        private readonly AppDbContext _context;

        public AdminSellerRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // الطلبات المعلقة أولًا ثم باقي الطلبات (السجل) من الأحدث للأقدم
        public async Task<IActionResult> Index()
        {
            var requests = await _context.SellerRequests
                .AsNoTracking()
                .OrderBy(r => r.Status == "Pending" ? 0 : 1)
                .ThenByDescending(r => r.CreatedAt)
                .Select(r => new AdminSellerRequestViewModel
                {
                    Id = r.Id,
                    UserName = r.user.Name,
                    UserEmail = r.user.Email,
                    ShopName = r.user.Seller != null ? r.user.Seller.ShopName : null,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ReviewedAt = r.ReviewdAt
                })
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            var request = await FindPendingRequestAsync(id);
            if (request == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var user = request.user;

            if (request.Role == null ||
                !string.Equals(request.Role.NormalizedName, "SELLER", StringComparison.OrdinalIgnoreCase))
            {
                TempData["AdminError"] = "Invalid seller request: the requested role is not a seller role.";
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(user.Role, "Administrator", StringComparison.OrdinalIgnoreCase))
            {
                TempData["AdminError"] = "Invalid seller request: administrators cannot become sellers.";
                return RedirectToAction(nameof(Index));
            }

            // 1) الطلب
            request.Status = "Approved";
            request.ReviewdAt = DateTime.Now;

            // 2) المستخدم
            user.Role = "Seller";

            // 3) البائع: تحديث الموجود أو إنشاء جديد
            var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.userId == user.Id);
            if (seller != null)
            {
                seller.IsApproved = 1;
                seller.Status = true;
            }
            else
            {
                _context.Sellers.Add(new Seller
                {
                    Id = Guid.NewGuid(),
                    userId = user.Id,
                    ShopName = $"{user.Name}'s Shop",
                    IsApproved = 1,
                    Status = true
                });
            }

            // 4) صف الـ Role (مطلوب لأن Product.RoleId إجباري)
            bool hasRoleRow = await _context.Roles
                .AnyAsync(r => r.userId == user.Id && r.netroleId == request.RoleId);

            if (!hasRoleRow)
            {
                _context.Roles.Add(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Seller",
                    userId = user.Id,
                    netroleId = request.RoleId
                });
            }

            return await SaveAsync($"{user.Name} has been approved as a seller.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id)
        {
            var request = await FindPendingRequestAsync(id);
            if (request == null)
            {
                return RedirectToAction(nameof(Index));
            }

            request.Status = "Rejected";
            request.ReviewdAt = DateTime.Now;

            var seller = await _context.Sellers.FirstOrDefaultAsync(s => s.userId == request.userId);
            if (seller != null)
            {
                seller.IsApproved = 0;
                seller.Status = false;
            }

            return await SaveAsync($"The seller request of {request.user.Name} has been rejected.");
        }

        // يرجع الطلب فقط إذا كان موجودًا وما زال Pending، وإلا يضع رسالة الخطأ المناسبة ويرجع null
        private async Task<SellerRequest?> FindPendingRequestAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["AdminError"] = "Invalid request id.";
                return null;
            }

            var request = await _context.SellerRequests
                .Include(r => r.user)
                .Include(r => r.Role)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                TempData["AdminError"] = "Seller request not found.";
                return null;
            }

            if (string.Equals(request.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                TempData["AdminError"] = "This seller request has already been approved.";
                return null;
            }

            if (string.Equals(request.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                TempData["AdminError"] = "This seller request has already been rejected.";
                return null;
            }

            if (!string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                TempData["AdminError"] = "This seller request has an unexpected status and cannot be processed.";
                return null;
            }

            return request;
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