using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtorsPortal.Data;
using RealtorsPortal.Models.Entities;

namespace RealtorsPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /Admin/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.PendingCount = await _db.Properties.CountAsync(p => p.ApprovalStatus == "Pending");
            ViewBag.ActiveCount  = await _db.Properties.CountAsync(p => p.ApprovalStatus == "Active");
            ViewBag.PendingBadge = await _db.Properties.CountAsync(p => p.ApprovalStatus == "Pending");
            return View();
        }

        // GET: /Admin/Listings
        [HttpGet]
        public async Task<IActionResult> Listings(string? status, int page = 1)
        {
            const int pageSize = 15;

            var query = _db.Properties
                .Include(p => p.Images)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "all")
                query = query.Where(p => p.ApprovalStatus == status);

            var total = await query.CountAsync();

            var properties = await query
                .OrderByDescending(p => p.ListedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Properties   = properties;
            ViewBag.Status       = status ?? "all";
            ViewBag.CurrentPage  = page;
            ViewBag.TotalPages   = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.TotalCount   = total;
            ViewBag.PendingCount  = await _db.Properties.CountAsync(p => p.ApprovalStatus == "Pending");
            ViewBag.ActiveCount   = await _db.Properties.CountAsync(p => p.ApprovalStatus == "Active");
            ViewBag.RejectedCount = await _db.Properties.CountAsync(p => p.ApprovalStatus == "Rejected");

            return View();
        }

        // POST: /Admin/ApproveListing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveListing(int id, string? returnStatus)
        {
            var prop = await _db.Properties.FindAsync(id);
            if (prop != null)
            {
                prop.ApprovalStatus = "Active";
                await _db.SaveChangesAsync();
                TempData["Success"] = $"'{prop.Title}' approved and is now live.";
            }
            return RedirectToAction(nameof(Listings), new { status = returnStatus ?? "Pending" });
        }

        // POST: /Admin/RejectListing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectListing(int id, string? returnStatus)
        {
            var prop = await _db.Properties.FindAsync(id);
            if (prop != null)
            {
                prop.ApprovalStatus = "Rejected";
                await _db.SaveChangesAsync();
                TempData["Success"] = $"'{prop.Title}' rejected.";
            }
            return RedirectToAction(nameof(Listings), new { status = returnStatus ?? "Pending" });
        }

        // ─── USERS ───────────────────────────────────────────────────────────────

        // GET: /Admin/Users
        [HttpGet]
        public async Task<IActionResult> Users(string? type, string? search)
        {
            var query = _db.Users
                .Where(u => u.AccountType == "Agent" || u.AccountType == "Seller")
                .AsQueryable();

            if (!string.IsNullOrEmpty(type) && type != "all")
                query = query.Where(u => u.AccountType == type);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.FullName.Contains(search) ||
                                        (u.Email != null && u.Email.Contains(search)));

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            var sellerListingCounts = await _db.Properties
                .Where(p => p.SellerId != null)
                .GroupBy(p => p.SellerId)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id!, x => x.Count);

            ViewBag.Users               = users;
            ViewBag.SellerListingCounts  = sellerListingCounts;
            ViewBag.SelectedType        = type ?? "all";
            ViewBag.Search              = search ?? "";
            ViewBag.TotalAgents         = await _db.Users.CountAsync(u => u.AccountType == "Agent");
            ViewBag.TotalSellers        = await _db.Users.CountAsync(u => u.AccountType == "Seller");
            ViewBag.PendingBadge        = await _db.Properties.CountAsync(p => p.ApprovalStatus == "Pending");
            return View();
        }

        // POST: /Admin/ToggleUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id, string? returnType)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            bool isLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
            if (isLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["Success"] = $"{user.FullName} unblocked.";
            }
            else
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                TempData["Success"] = $"{user.FullName} blocked.";
            }

            return RedirectToAction(nameof(Users), new { type = returnType });
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id, string? returnType)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var name = user.FullName;
            await _userManager.DeleteAsync(user);
            TempData["Success"] = $"{name} deleted.";

            return RedirectToAction(nameof(Users), new { type = returnType });
        }
    }

}
