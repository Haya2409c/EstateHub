using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtorsPortal.Data;
using RealtorsPortal.Models.Entities;
using RealtorsPortal.Models.ViewModels;

namespace RealtorsPortal.Controllers
{
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public AgentController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        // ─── DASHBOARD ────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var allProps = await _db.Properties
                .Where(p => p.SellerId == userId)
                .Include(p => p.Images)
                .OrderByDescending(p => p.ListedDate)
                .ToListAsync();

            var totalLeads = await _db.PropertyEnquiries
                .CountAsync(e => e.OwnerUserId == userId);
            var unreadLeads = await _db.PropertyEnquiries
                .CountAsync(e => e.OwnerUserId == userId && !e.IsRead);

            ViewBag.TotalProperties  = allProps.Count;
            ViewBag.ActiveListings   = allProps.Count(p => p.ApprovalStatus == "Active");
            ViewBag.TotalLeads       = totalLeads;
            ViewBag.UnreadLeads      = unreadLeads;
            ViewBag.RecentProperties = allProps.Take(5).ToList();
            ViewBag.RecentLeads      = await _db.PropertyEnquiries
                .Where(e => e.OwnerUserId == userId)
                .Include(e => e.Property)
                .OrderByDescending(e => e.SubmittedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }

        // ─── MY LISTINGS ──────────────────────────────────────────────────────────

        public async Task<IActionResult> MyListings()
        {
            var userId = _userManager.GetUserId(User);
            var properties = await _db.Properties
                .Where(p => p.SellerId == userId)
                .Include(p => p.Images)
                .OrderByDescending(p => p.ListedDate)
                .ToListAsync();
            return View(properties);
        }

        // ─── CREATE LISTING ───────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult CreateListing() => View(new CreateListingViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> CreateListing(CreateListingViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);

            var typeSlug = model.PropertyType.ToLower();
            var category = await _db.Categories
                               .FirstOrDefaultAsync(c => c.Slug == typeSlug || c.Name.ToLower() == typeSlug)
                           ?? await _db.Categories.FirstOrDefaultAsync();

            if (category == null)
            {
                ModelState.AddModelError("", "No property categories configured. Please contact support.");
                return View(model);
            }

            var property = new Property
            {
                Title          = model.Title,
                Description    = model.Description,
                PropertyType   = model.PropertyType,
                Status         = model.Status,
                ApprovalStatus = "Active",
                Price          = model.Price,
                Bedrooms       = model.Bedrooms,
                Bathrooms      = model.Bathrooms,
                AreaSqft       = model.AreaSqft,
                Address        = $"{model.Address}, {model.City}, {model.Region}".Trim(',', ' '),
                Amenities      = model.Amenities.Any() ? string.Join(",", model.Amenities) : null,
                SellerId       = userId,
                CategoryId     = category.Id,
                ListedDate     = DateTime.UtcNow
            };

            _db.Properties.Add(property);
            await _db.SaveChangesAsync();

            property.Slug = System.Text.RegularExpressions.Regex
                .Replace(model.Title.ToLower().Replace(" ", "-"), @"[^a-z0-9\-]", "")
                + "-" + property.Id;
            await _db.SaveChangesAsync();

            await SaveImagesAsync(property.Id, model.Images);

            TempData["Success"] = "Property published successfully!";
            return RedirectToAction(nameof(MyListings));
        }

        // ─── EDIT LISTING ─────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EditListing(int id)
        {
            var userId = _userManager.GetUserId(User);
            var property = await _db.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == userId);
            if (property == null) return NotFound();

            var vm = new CreateListingViewModel
            {
                Title        = property.Title,
                Description  = property.Description ?? "",
                PropertyType = property.PropertyType ?? "house",
                Status       = property.Status ?? "buy",
                Price        = property.Price,
                Bedrooms     = property.Bedrooms,
                Bathrooms    = property.Bathrooms,
                AreaSqft     = property.AreaSqft,
                Address      = property.Address ?? "",
                Amenities    = property.Amenities?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new()
            };

            ViewBag.PropertyId     = id;
            ViewBag.ExistingImages = property.Images.OrderBy(i => i.SortOrder).ToList();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditListing(int id, CreateListingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PropertyId = id;
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var property = await _db.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == userId);
            if (property == null) return NotFound();

            var typeSlug = model.PropertyType.ToLower();
            var category = await _db.Categories
                               .FirstOrDefaultAsync(c => c.Slug == typeSlug || c.Name.ToLower() == typeSlug)
                           ?? await _db.Categories.FirstOrDefaultAsync();

            if (category == null)
            {
                ViewBag.PropertyId = id;
                ModelState.AddModelError("", "No property categories configured. Please contact support.");
                return View(model);
            }

            property.Title        = model.Title;
            property.Description  = model.Description;
            property.PropertyType = model.PropertyType;
            property.Status       = model.Status;
            property.Price        = model.Price;
            property.Bedrooms     = model.Bedrooms;
            property.Bathrooms    = model.Bathrooms;
            property.AreaSqft     = model.AreaSqft;
            property.Address      = model.Address != property.Address
                                      ? $"{model.Address}, {model.City}, {model.Region}".Trim(',', ' ')
                                      : property.Address;
            property.Amenities    = model.Amenities.Any() ? string.Join(",", model.Amenities) : null;
            property.CategoryId   = category.Id;

            await _db.SaveChangesAsync();
            await SaveImagesAsync(property.Id, model.Images, existingCount: property.Images.Count);

            TempData["Success"] = "Property updated successfully!";
            return RedirectToAction(nameof(MyListings));
        }

        // ─── DELETE LISTING ───────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteListing(int id)
        {
            var userId = _userManager.GetUserId(User);
            var property = await _db.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == userId);

            if (property != null)
            {
                foreach (var img in property.Images)
                {
                    var filePath = Path.Combine(_env.WebRootPath, img.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }
                _db.Properties.Remove(property);
                await _db.SaveChangesAsync();
            }

            TempData["Success"] = "Property deleted.";
            return RedirectToAction(nameof(MyListings));
        }

        // ─── UPLOAD IMAGES ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> UploadImages(int? listingId = null)
        {
            var userId = _userManager.GetUserId(User);
            var properties = await _db.Properties
                .Where(p => p.SellerId == userId)
                .OrderByDescending(p => p.ListedDate)
                .ToListAsync();

            ViewBag.Properties = properties;
            ViewBag.SelectedId = listingId;

            List<PropertyImage> images = new();
            Property? selectedProp = null;
            if (listingId.HasValue)
            {
                selectedProp = await _db.Properties
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == listingId && p.SellerId == userId);
                if (selectedProp != null)
                    images = selectedProp.Images.OrderBy(i => i.SortOrder).ToList();
            }

            ViewBag.SelectedProperty = selectedProp;
            ViewBag.ExistingImages   = images;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadImagesConfirmed(int? listingId, List<IFormFile> Images)
        {
            if (!listingId.HasValue)
            {
                TempData["Error"] = "No property selected.";
                return RedirectToAction(nameof(UploadImages));
            }

            if (Images == null || Images.Count == 0)
            {
                TempData["Error"] = "No images were received by the server. Please select files and try again.";
                return RedirectToAction(nameof(UploadImages), new { listingId });
            }

            var userId = _userManager.GetUserId(User);
            var property = await _db.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == listingId && p.SellerId == userId);

            if (property == null)
            {
                TempData["Error"] = "Property not found or you do not have permission to upload images for it.";
                return RedirectToAction(nameof(UploadImages), new { listingId });
            }

            try
            {
                await SaveImagesAsync(property.Id, Images, existingCount: property.Images.Count);
                TempData["Success"] = $"{Images.Count} image(s) uploaded successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Upload failed: {ex.Message}";
            }

            return RedirectToAction(nameof(UploadImages), new { listingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int? listingId)
        {
            var userId = _userManager.GetUserId(User);
            var image = await _db.PropertyImages
                .Include(i => i.Property)
                .FirstOrDefaultAsync(i => i.Id == imageId && i.Property != null && i.Property.SellerId == userId);

            if (image != null)
            {
                var filePath = Path.Combine(_env.WebRootPath, image.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                _db.PropertyImages.Remove(image);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Image deleted.";
            }
            return RedirectToAction(nameof(UploadImages), new { listingId });
        }

        // ─── LEADS ────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Leads(string? status, int? propertyId, int page = 1)
        {
            const int pageSize = 15;
            var userId = _userManager.GetUserId(User);

            var query = _db.PropertyEnquiries
                .Where(e => e.OwnerUserId == userId)
                .Include(e => e.Property)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "all")
                query = query.Where(e => e.IsRead == (status == "read"));

            if (propertyId.HasValue)
                query = query.Where(e => e.PropertyId == propertyId.Value);

            int total = await query.CountAsync();
            var leads = await query
                .OrderByDescending(e => e.SubmittedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var myProperties = await _db.Properties
                .Where(p => p.SellerId == userId)
                .Select(p => new { p.Id, p.Title })
                .ToListAsync();

            ViewBag.Leads        = leads;
            ViewBag.TotalLeads   = total;
            ViewBag.TotalPages   = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.CurrentPage  = page;
            ViewBag.MyProperties = myProperties;
            ViewBag.SelectedStatus     = status ?? "all";
            ViewBag.SelectedPropertyId = propertyId;
            ViewBag.UnreadCount  = await _db.PropertyEnquiries.CountAsync(e => e.OwnerUserId == userId && !e.IsRead);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkLeadRead(int id)
        {
            var userId = _userManager.GetUserId(User);
            var lead = await _db.PropertyEnquiries
                .FirstOrDefaultAsync(e => e.Id == id && e.OwnerUserId == userId);
            if (lead != null) { lead.IsRead = true; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Leads));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLead(int id)
        {
            var userId = _userManager.GetUserId(User);
            var lead = await _db.PropertyEnquiries
                .FirstOrDefaultAsync(e => e.Id == id && e.OwnerUserId == userId);
            if (lead != null) { _db.PropertyEnquiries.Remove(lead); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Lead deleted.";
            return RedirectToAction(nameof(Leads));
        }

        // ─── PACKAGES ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Packages()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _db.SellerProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            if (profile == null)
            {
                profile = new SellerProfile { UserId = userId!, PackageTier = "Basic", CreatedAt = DateTime.UtcNow };
                _db.SellerProfiles.Add(profile);
                await _db.SaveChangesAsync();
            }

            var totalProps = await _db.Properties.CountAsync(p => p.SellerId == userId);
            int maxListings = profile.PackageTier switch { "Gold" => 50, "Platinum" => int.MaxValue, _ => 10 };

            ViewBag.Profile      = profile;
            ViewBag.TotalProps   = totalProps;
            ViewBag.MaxListings  = maxListings == int.MaxValue ? "∞" : maxListings.ToString();
            ViewBag.UsedPercent  = maxListings == int.MaxValue ? 0 : (int)((double)totalProps / maxListings * 100);
            return View();
        }

        // ─── PROFILE ──────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Profile() => View(new ProfileUpdateViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileUpdateViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(model.FullName) && model.FullName != user.FullName)
                user.FullName = model.FullName;

            if (model.Phone != user.PhoneNumber)
                await _userManager.SetPhoneNumberAsync(user, model.Phone);

            if (model.Photo is { Length: > 0 })
            {
                var ext = Path.GetExtension(model.Photo.FileName).ToLowerInvariant();
                if (ext is ".jpg" or ".jpeg" or ".png" or ".webp")
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(uploadsDir);
                    var fileName = $"agent_{user.Id}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await model.Photo.CopyToAsync(stream);
                    user.PhotoUrl = $"/uploads/profiles/{fileName}";
                }
                else
                {
                    errors.Add("Only JPG, PNG, or WebP images are allowed.");
                }
            }

            if (!errors.Any())
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    errors.AddRange(updateResult.Errors.Select(e => e.Description));
            }

            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!result.Succeeded)
                    errors.AddRange(result.Errors.Select(e => e.Description));
            }

            if (errors.Any())
                TempData["Error"] = string.Join(" ", errors);
            else
                TempData["Success"] = "Profile updated successfully!";

            return RedirectToAction(nameof(Profile));
        }

        // ─── NOTIFICATIONS ────────────────────────────────────────────────────────

        public async Task<IActionResult> Notifications()
        {
            var userId = _userManager.GetUserId(User);
            var recentLeads = await _db.PropertyEnquiries
                .Where(e => e.OwnerUserId == userId)
                .Include(e => e.Property)
                .OrderByDescending(e => e.SubmittedAt)
                .Take(20)
                .ToListAsync();

            ViewBag.Notifications = recentLeads;
            ViewBag.UnreadCount   = recentLeads.Count(l => !l.IsRead);
            return View();
        }

        // ─── HELPERS ──────────────────────────────────────────────────────────────

        private async Task SaveImagesAsync(int propertyId, List<IFormFile>? files, int existingCount = 0)
        {
            if (files == null || files.Count == 0) return;
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "properties");
            Directory.CreateDirectory(uploadsDir);
            int sortOrder = existingCount;
            foreach (var file in files)
            {
                if (file.Length == 0) continue;
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp")) continue;
                var fileName = $"{propertyId}_{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                _db.PropertyImages.Add(new PropertyImage
                {
                    PropertyId = propertyId,
                    Url        = $"/uploads/properties/{fileName}",
                    IsPrimary  = existingCount == 0 && sortOrder == 0,
                    SortOrder  = sortOrder++
                });
            }
            await _db.SaveChangesAsync();
        }
    }
}
