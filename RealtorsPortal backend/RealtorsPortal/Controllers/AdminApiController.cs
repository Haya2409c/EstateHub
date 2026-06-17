using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtorsPortal.Data;
using RealtorsPortal.Models.Entities;
using System.Security.Claims;

namespace RealtorsPortal.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminApiController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ─────────────────────────────────────────────
        // SETTINGS  /api/admin/settings
        // ─────────────────────────────────────────────

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _db.SiteSettings
                .OrderBy(s => s.Group).ThenBy(s => s.Key)
                .ToListAsync();

            var dict = settings.ToDictionary(s => s.Key, s => s.Value);
            return Ok(dict);
        }

        // PUT /api/admin/settings  — body: { "currency": "USD", "page_size": "20", ... }
        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] Dictionary<string, string> updates)
        {
            if (updates is null || updates.Count == 0)
                return BadRequest(new { error = "No settings provided." });

            var keys = updates.Keys.ToList();
            var existing = await _db.SiteSettings
                .Where(s => keys.Contains(s.Key))
                .ToListAsync();

            foreach (var kv in updates)
            {
                var row = existing.FirstOrDefault(s => s.Key == kv.Key);
                if (row is not null)
                {
                    row.Value = kv.Value;
                    row.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _db.SiteSettings.Add(new SiteSetting
                    {
                        Key = kv.Key,
                        Value = kv.Value,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Settings saved." });
        }

        // ─────────────────────────────────────────────
        // PACKAGES  /api/admin/packages
        // ─────────────────────────────────────────────

        [HttpGet("packages")]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _db.Packages
                .OrderBy(p => p.SortOrder)
                .Select(p => new
                {
                    p.Id, p.Name, p.Description, p.Price,
                    p.DurationDays, p.ListingLimit, p.ImageLimit,
                    p.IsActive, p.IsFeatured, p.SortOrder,
                    p.CreatedAt, p.UpdatedAt
                })
                .ToListAsync();

            return Ok(packages);
        }

        [HttpGet("packages/{id:int}")]
        public async Task<IActionResult> GetPackage(int id)
        {
            var pkg = await _db.Packages.FindAsync(id);
            return pkg is null ? NotFound() : Ok(pkg);
        }

        [HttpPost("packages")]
        public async Task<IActionResult> CreatePackage([FromBody] PackageDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var pkg = new Package
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DurationDays = dto.DurationDays,
                ListingLimit = dto.ListingLimit,
                ImageLimit = dto.ImageLimit,
                IsActive = dto.IsActive,
                IsFeatured = dto.IsFeatured,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Packages.Add(pkg);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPackage), new { id = pkg.Id }, pkg);
        }

        [HttpPut("packages/{id:int}")]
        public async Task<IActionResult> UpdatePackage(int id, [FromBody] PackageDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var pkg = await _db.Packages.FindAsync(id);
            if (pkg is null) return NotFound();

            pkg.Name = dto.Name;
            pkg.Description = dto.Description;
            pkg.Price = dto.Price;
            pkg.DurationDays = dto.DurationDays;
            pkg.ListingLimit = dto.ListingLimit;
            pkg.ImageLimit = dto.ImageLimit;
            pkg.IsActive = dto.IsActive;
            pkg.IsFeatured = dto.IsFeatured;
            pkg.SortOrder = dto.SortOrder;
            pkg.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(pkg);
        }

        [HttpDelete("packages/{id:int}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var pkg = await _db.Packages.FindAsync(id);
            if (pkg is null) return NotFound();

            bool inUse = await _db.SubscriptionHistories.AnyAsync(s => s.PackageId == id);
            if (inUse)
                return Conflict(new { error = "Package has active subscriptions and cannot be deleted." });

            _db.Packages.Remove(pkg);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ─────────────────────────────────────────────
        // LISTINGS  /api/admin/listings
        // ─────────────────────────────────────────────

        [HttpGet("listings")]
        public async Task<IActionResult> GetListings(
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);

            var query = _db.Properties
                .Include(p => p.Category)
                .Include(p => p.Agent)
                .Include(p => p.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.ApprovalStatus == status);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.ListedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id, p.Title, p.Slug, p.Price,
                    ListingType    = p.Status,          // buy / rent / sell
                    ApprovalStatus = p.ApprovalStatus,  // Pending / Active / Rejected
                    p.PropertyType, p.IsFeatured, p.ListedDate,
                    p.SellerId, p.AgentId,
                    Category = p.Category == null ? null : p.Category.Name,
                    Agent    = p.Agent    == null ? null : p.Agent.FullName,
                    ThumbnailUrl = p.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.Url)
                        .FirstOrDefault()
                        ?? p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // PUT /api/admin/listings/{id}/approve
        [HttpPut("listings/{id:int}/approve")]
        public async Task<IActionResult> ApproveListing(int id)
        {
            var prop = await _db.Properties.FindAsync(id);
            if (prop is null) return NotFound();

            prop.ApprovalStatus = "Active";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Listing approved.", id, status = prop.ApprovalStatus });
        }

        // PUT /api/admin/listings/{id}/reject
        [HttpPut("listings/{id:int}/reject")]
        public async Task<IActionResult> RejectListing(int id, [FromBody] RejectDto? dto)
        {
            var prop = await _db.Properties.FindAsync(id);
            if (prop is null) return NotFound();

            prop.ApprovalStatus = "Rejected";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Listing rejected.", id, status = prop.ApprovalStatus });
        }

        // DELETE /api/admin/listings/{id}
        [HttpDelete("listings/{id:int}")]
        public async Task<IActionResult> DeleteListing(int id)
        {
            var prop = await _db.Properties.FindAsync(id);
            if (prop is null) return NotFound();

            _db.Properties.Remove(prop);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ─────────────────────────────────────────────
        // USERS  /api/admin/users
        // ─────────────────────────────────────────────

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? accountType,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);

            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(accountType))
                query = query.Where(u => u.AccountType == accountType);

            if (isActive.HasValue)
                query = query.Where(u => !u.LockoutEnabled || (isActive.Value
                    ? u.LockoutEnd == null || u.LockoutEnd < DateTimeOffset.UtcNow
                    : u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow));

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName.Contains(search) || (u.Email != null && u.Email.Contains(search)));

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id, u.FullName, u.Email, u.PhoneNumber,
                    u.AccountType, u.PhotoUrl, u.CreatedAt,
                    IsLocked = u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _db.Users
                .Include(u => u.SellerProfile)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user is null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id, user.FullName, user.Email, user.PhoneNumber,
                user.AccountType, user.PhotoUrl, user.CreatedAt,
                IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                Roles = roles,
                SellerProfile = user.SellerProfile is null ? null : new
                {
                    user.SellerProfile.CompanyName,
                    user.SellerProfile.PackageTier,
                    user.SellerProfile.PackageExpiry
                }
            });
        }

        // PUT /api/admin/users/{id}/toggle-status  — lock or unlock
        [HttpPut("users/{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            bool isCurrentlyLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;

            if (isCurrentlyLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                return Ok(new { message = "User unlocked.", isLocked = false });
            }
            else
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                return Ok(new { message = "User locked.", isLocked = true });
            }
        }

        // DELETE /api/admin/users/{id}
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            return NoContent();
        }

        // ─────────────────────────────────────────────
        // REPORTS  /api/admin/reports
        // ─────────────────────────────────────────────

        // GET /api/admin/reports/transactions?from=2026-01-01&to=2026-12-31&status=Completed
        [HttpGet("reports/transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            pageSize = Math.Clamp(pageSize, 1, 200);
            page = Math.Max(1, page);

            var query = _db.Payments
                .Include(p => p.User)
                .Include(p => p.Package)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(p => p.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(p => p.CreatedAt <= to.Value.AddDays(1));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status == status);

            var total = await query.CountAsync();

            var totalRevenue = await query
                .Where(p => p.Status == "Completed")
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.TransactionId,
                    p.Amount,
                    p.Currency,
                    p.Status,
                    p.CreatedAt,
                    Package = p.Package.Name,
                    CustomerName = p.User.FullName,
                    CustomerEmail = p.User.Email
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, totalRevenue, items });
        }

        // GET /api/admin/reports/summary  — dashboard stat cards
        [HttpGet("reports/summary")]
        public async Task<IActionResult> GetSummary()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var totalRevenue = await _db.Payments
                .Where(p => p.Status == "Completed")
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var monthRevenue = await _db.Payments
                .Where(p => p.Status == "Completed" && p.CreatedAt >= monthStart)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var totalUsers = await _db.Users.CountAsync();
            var totalSellers = await _db.Users.CountAsync(u => u.AccountType == "Seller");
            var totalAgents = await _db.Users.CountAsync(u => u.AccountType == "Agent");
            var totalListings = await _db.Properties.CountAsync();
            var pendingListings = await _db.Properties.CountAsync(p => p.ApprovalStatus == "Pending");
            var activeSubscriptions = await _db.SubscriptionHistories
                .CountAsync(s => s.Status == "Active" && s.EndDate > now);

            return Ok(new
            {
                totalRevenue,
                monthRevenue,
                totalUsers,
                totalSellers,
                totalAgents,
                totalListings,
                pendingListings,
                activeSubscriptions
            });
        }

        // GET /api/admin/reports/revenue-monthly?year=2026
        [HttpGet("reports/revenue-monthly")]
        public async Task<IActionResult> GetMonthlyRevenue([FromQuery] int? year)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;

            var rows = await _db.Payments
                .Where(p => p.Status == "Completed" && p.CreatedAt.Year == targetYear)
                .GroupBy(p => p.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(p => p.Amount) })
                .ToListAsync();

            var monthly = Enumerable.Range(1, 12)
                .Select(m => rows.FirstOrDefault(r => r.Month == m)?.Total ?? 0m)
                .ToArray();

            return Ok(new { year = targetYear, monthly });
        }

        // GET /api/admin/reports/subscriptions
        [HttpGet("reports/subscriptions")]
        public async Task<IActionResult> GetSubscriptions(
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(1, page);

            var query = _db.SubscriptionHistories
                .Include(s => s.User)
                .Include(s => s.Package)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(s => s.Status == status);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.Id, s.Status, s.StartDate, s.EndDate, s.CreatedAt,
                    Package = s.Package.Name,
                    UserName = s.User.FullName,
                    UserEmail = s.User.Email
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // ─────────────────────────────────────────────
        // PROFILE  /api/admin/profile
        // ─────────────────────────────────────────────

        // PUT /api/admin/profile/change-password
        [HttpPut("profile/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user is null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { error = string.Join(", ", result.Errors.Select(e => e.Description)) });

            return Ok(new { message = "Password changed successfully." });
        }
    }

    // ─── DTOs ────────────────────────────────────────
    public sealed class PackageDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public int? ListingLimit { get; set; }
        public int? ImageLimit { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }

    public sealed class RejectDto
    {
        public string? Reason { get; set; }
    }

    public sealed class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
