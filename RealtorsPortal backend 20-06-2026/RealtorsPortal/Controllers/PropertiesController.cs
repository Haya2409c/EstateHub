using Microsoft.AspNetCore.Authorization;
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
                .Where(p => p.ApprovalStatus == "Active")
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
                    ?? p.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url,
                ListedDate = p.ListedDate
            }).ToList();

            var rawCats = await _db.Categories
                .Select(c => new { c.Name, Slug = c.Slug ?? c.Name.ToLower(), Count = c.Properties.Count(p => p.ApprovalStatus == "Active") })
                .OrderBy(c => c.Name)
                .ToListAsync();

            var cityNames = await _db.Cities
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .ToListAsync();

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
                    SortBy = sort,
                    PropertyTypeOptions = rawCats.Select(c => new FilterOptionViewModel
                    {
                        Value = c.Slug,
                        Label = c.Name,
                        Count = c.Count
                    }).ToList(),
                    LocationOptions = cityNames.Select(name => new FilterOptionViewModel
                    {
                        Value = name,
                        Label = name
                    }).ToList()
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
                .Include(p => p.Agent)
                .FirstOrDefaultAsync(p => p.Id == id && p.ApprovalStatus == "Active");

            if (p == null) return NotFound();

            var similar = await _db.Properties
                .Include(s => s.Images)
                .Include(s => s.Category)
                .Where(s => s.Id != id && s.ApprovalStatus == "Active" && s.CategoryId == p.CategoryId)
                .OrderByDescending(s => s.ListedDate)
                .Take(3)
                .ToListAsync();

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
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                ListedDate = p.ListedDate,
                ImageGallery = p.Images.Select(i => new PropertyImageViewModel
                {
                    Id = i.Id,
                    Url = i.Url,
                    IsPrimary = i.IsPrimary,
                    SortOrder = i.SortOrder
                }).ToList(),
                ListingAgent = p.Agent == null ? null : new RealtorsPortal.Models.ViewModels.Shared.AgentCardViewModel
                {
                    Id = p.Agent.Id,
                    Slug = p.Agent.Slug,
                    FullName = p.Agent.FullName,
                    Email = p.Agent.Email,
                    Phone = p.Agent.Phone,
                    PhotoUrl = p.Agent.PhotoUrl,
                    Bio = p.Agent.Bio,
                    YearsExperience = p.Agent.YearsExperience,
                    Rating = p.Agent.Rating
                },
                SimilarProperties = similar.Select(s => new PropertyCardViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Price = s.Price,
                    PropertyType = s.Category?.Name,
                    Bedrooms = s.Bedrooms,
                    Bathrooms = s.Bathrooms,
                    AreaSqft = s.AreaSqft,
                    Address = s.Address,
                    Status = s.Status,
                    ListedDate = s.ListedDate,
                    ThumbnailUrl = s.Images.FirstOrDefault(i => i.IsPrimary)?.Url
                        ?? s.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url
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
        public async Task<IActionResult> AdvancedSearch(
            string? keyword, string? city, string? type, string? status,
            decimal? minprice, decimal? maxprice, int? beds, int? baths,
            decimal? minarea, decimal? maxarea, List<string>? amenities, int page = 1)
        {
            const int pageSize = 9;

            var rawCats = await _db.Categories
                .Select(c => new { c.Name, Slug = c.Slug ?? c.Name.ToLower(),
                    Count = c.Properties.Count(p => p.ApprovalStatus == "Active") })
                .OrderBy(c => c.Name).ToListAsync();

            var cityNames = await _db.Cities.OrderBy(c => c.Name).Select(c => c.Name).ToListAsync();

            bool hasSearched = !string.IsNullOrWhiteSpace(keyword)
                || !string.IsNullOrWhiteSpace(city)
                || !string.IsNullOrWhiteSpace(type)
                || !string.IsNullOrWhiteSpace(status)
                || minprice.HasValue || maxprice.HasValue
                || beds.HasValue || baths.HasValue
                || minarea.HasValue || maxarea.HasValue
                || amenities?.Any(a => !string.IsNullOrWhiteSpace(a)) == true;

            List<PropertyCardViewModel> results = new();
            int total = 0, totalPages = 0;

            if (hasSearched)
            {
                var query = _db.Properties
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .Where(p => p.ApprovalStatus == "Active")
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                    query = query.Where(p => p.Title.Contains(keyword) ||
                        (p.Description != null && p.Description.Contains(keyword)));

                if (!string.IsNullOrWhiteSpace(city))
                    query = query.Where(p => p.Address != null && p.Address.ToLower().Contains(city.ToLower()));

                if (!string.IsNullOrWhiteSpace(type))
                    query = query.Where(p => p.Category != null && p.Category.Slug == type);

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(p => p.Status == status);

                if (minprice.HasValue) query = query.Where(p => p.Price >= minprice.Value);
                if (maxprice.HasValue) query = query.Where(p => p.Price <= maxprice.Value);
                if (beds.HasValue)     query = query.Where(p => p.Bedrooms >= beds.Value);
                if (baths.HasValue)    query = query.Where(p => p.Bathrooms >= baths.Value);
                if (minarea.HasValue)  query = query.Where(p => p.AreaSqft >= minarea.Value);
                if (maxarea.HasValue)  query = query.Where(p => p.AreaSqft <= maxarea.Value);

                if (amenities != null)
                {
                    foreach (var tag in amenities.Where(a => !string.IsNullOrWhiteSpace(a)))
                        query = query.Where(p => p.Amenities != null && p.Amenities.Contains(tag));
                }

                query = query.OrderByDescending(p => p.ListedDate);

                total = await query.CountAsync();
                totalPages = (int)Math.Ceiling(total / (double)pageSize);

                var props = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                results = props.Select(p => new PropertyCardViewModel
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
                    ListedDate = p.ListedDate,
                    ThumbnailUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.Url
                        ?? p.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url
                }).ToList();
            }

            var vm = new AdvancedSearchViewModel
            {
                Filters = new PropertyFilterViewModel
                {
                    Keyword = keyword,
                    Location = city,
                    PropertyType = type,
                    Status = status,
                    MinPrice = minprice,
                    MaxPrice = maxprice,
                    MinBedrooms = beds,
                    MinBathrooms = baths,
                    MinArea = minarea,
                    MaxArea = maxarea,
                    Amenities = amenities?.Where(a => !string.IsNullOrWhiteSpace(a)).ToList() ?? new(),
                    LocationOptions = cityNames.Select(n => new FilterOptionViewModel { Value = n, Label = n }).ToList(),
                    PropertyTypeOptions = rawCats.Select(c => new FilterOptionViewModel
                        { Value = c.Slug, Label = c.Name, Count = c.Count }).ToList()
                },
                HasSearched = hasSearched,
                Results = results,
                ResultsCount = total,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalItems = total,
                    PageSize = pageSize
                }
            };

            ViewBag.ActivePage = "Properties";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactAgent(PropertyEnquiryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var property = await _db.Properties.FindAsync(model.PropertyId);
            if (property == null) return NotFound();

            var enquiry = new RealtorsPortal.Models.Entities.PropertyEnquiry
            {
                PropertyId = model.PropertyId,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Message = model.Message,
                SubmittedAt = DateTime.UtcNow,
                OwnerUserId = property.SellerId
            };
            _db.PropertyEnquiries.Add(enquiry);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Your enquiry has been submitted successfully.";
            return RedirectToAction("Details", new { id = model.PropertyId });
        }
    }
}
