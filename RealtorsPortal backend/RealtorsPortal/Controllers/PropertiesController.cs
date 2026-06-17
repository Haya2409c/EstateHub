using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtorsPortal.Data;
using RealtorsPortal.Models.ViewModels.Properties;
using RealtorsPortal.Models.ViewModels.Shared;
using System.Text.Json;

namespace RealtorsPortal.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PropertiesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Properties
        public async Task<IActionResult> Index(string? keyword, string? type, string? city,
            string? status, decimal? minprice, decimal? maxprice, int? beds, int? baths,
            string? sort, int page = 1)
        {
            const int pageSize = 9;

            var query = _db.Properties
                .Include(p => p.Images)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Title.Contains(keyword) || (p.Description != null && p.Description.Contains(keyword)));

            if (!string.IsNullOrWhiteSpace(type) && type != "all")
                query = query.Where(p => p.Category != null && p.Category.Slug == type);

            if (!string.IsNullOrWhiteSpace(city) && city != "all")
                query = query.Where(p => p.Address != null && p.Address.ToLower().Contains(city.ToLower()));

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
                query = query.Where(p => p.Status == status);

            if (minprice.HasValue)
                query = query.Where(p => p.Price >= minprice.Value);

            if (maxprice.HasValue)
                query = query.Where(p => p.Price <= maxprice.Value);

            if (beds.HasValue)
                query = query.Where(p => p.Bedrooms >= beds.Value);

            if (baths.HasValue)
                query = query.Where(p => p.Bathrooms >= baths.Value);

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.ListedDate)
            };

            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var properties = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var cards = properties.Select(p => new PropertyCardViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                PropertyType = p.Category?.Name,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                AreaSqft = p.AreaSqft,
                Address = p.Address,
                Status = p.Status,
                ThumbnailUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.Url
                    ?? p.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url
                    ?? "/assets/images/new-property-1.jpg",
                ListedDate = p.ListedDate
            }).ToList();

            var vm = new PropertyListViewModel
            {
                Properties = cards,
                Filter = new PropertyFilterViewModel
                {
                    Keyword = keyword,
                    PropertyType = type,
                    MinPrice = minprice,
                    MaxPrice = maxprice,
                    MinBedrooms = beds,
                    MinBathrooms = baths,
                    SortBy = sort
                },
                Pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalItems = total,
                    PageSize = pageSize
                }
            };

            ViewBag.ActivePage = "Properties";
            ViewBag.SelectedCity = city ?? "all";
            ViewBag.SelectedStatus = status ?? "all";
            return View(vm);
        }

        // GET: /Properties/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var p = await _db.Properties
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null) return NotFound();

            var vm = new PropertyDetailsViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                PropertyType = p.Category?.Name,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                AreaSqft = p.AreaSqft,
                Address = p.Address,
                ListedDate = p.ListedDate,
                ImageGallery = p.Images.Select(i => new PropertyImageViewModel
                {
                    Id = i.Id,
                    Url = i.Url,
                    IsPrimary = i.IsPrimary,
                    SortOrder = i.SortOrder
                }).ToList()
            };

            ViewBag.Status = p.Status;
            ViewBag.Amenities = string.IsNullOrEmpty(p.Amenities)
                ? new List<string>()
                : p.Amenities.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            ViewBag.ActivePage = "Properties";
            return View(vm);
        }

        // GET: /Properties/AdvancedSearch
        public IActionResult AdvancedSearch()
        {
            ViewBag.ActivePage = "Properties";
            return View(new AdvancedSearchViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactAgent(PropertyEnquiryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var property = await _db.Properties.FindAsync(model.PropertyId);
            var enquiry = new RealtorsPortal.Models.Entities.PropertyEnquiry
            {
                PropertyId = model.PropertyId,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Message = model.Message,
                SubmittedAt = DateTime.UtcNow,
                OwnerUserId = property?.SellerId
            };
            _db.PropertyEnquiries.Add(enquiry);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Your enquiry has been submitted successfully.";
            return RedirectToAction("Details", new { id = model.PropertyId });
        }
    }
}
