using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtorsPortal.Data;
using RealtorsPortal.Models.ViewModels.Home;
using RealtorsPortal.Models.ViewModels.Shared;

namespace RealtorsPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        private static readonly Dictionary<string, string> _categoryImages = new(StringComparer.OrdinalIgnoreCase)
        {
            ["house"]      = "https://images.unsplash.com/photo-1613490493576-7fde63acd811?auto=format&fit=crop&w=600&q=80",
            ["apartment"]  = "https://images.unsplash.com/photo-1560185007-cde436f6a4d0?auto=format&fit=crop&w=600&q=80",
            ["villa"]      = "https://images.unsplash.com/photo-1571939228382-b2f2b585ce15?auto=format&fit=crop&w=600&q=80",
            ["commercial"] = "https://images.unsplash.com/photo-1570129477492-45c003edd2be?auto=format&fit=crop&w=600&q=80",
            ["land"]       = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=600&q=80",
            ["plot"]       = "https://images.unsplash.com/photo-1500382017468-9049fed747ef?auto=format&fit=crop&w=600&q=80",
        };

        private const string _defaultCategoryImage =
            "https://images.unsplash.com/photo-1560184897-ae75f418493e?auto=format&fit=crop&w=600&q=80";

        // GET: /
        public async Task<IActionResult> Index()
        {
            var recentProperties = await _db.Properties
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Where(p => p.ApprovalStatus == "Active")
                .OrderByDescending(p => p.ListedDate)
                .Take(6)
                .ToListAsync();

            var featured = recentProperties.Select(p => new PropertyCardViewModel
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

            var totalProperties = await _db.Properties.CountAsync(p => p.ApprovalStatus == "Active");
            var totalAgents = await _db.Agents.CountAsync();
            var totalCities = await _db.Cities.CountAsync();
            var totalEnquiries = await _db.PropertyEnquiries.CountAsync();

            var topAgentRows = await _db.Users
                .Where(u => u.AccountType == "Agent")
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.PhotoUrl,
                    ListingCount = _db.Properties.Count(p => p.SellerId == u.Id && p.ApprovalStatus == "Active")
                })
                .OrderByDescending(u => u.ListingCount)
                .Take(3)
                .ToListAsync();

            var topAgents = topAgentRows.Select(u => new AgentCardViewModel
            {
                Slug             = u.Id,
                FullName         = u.FullName ?? "Agent",
                PhotoUrl         = u.PhotoUrl,
                PropertiesCount  = u.ListingCount
            }).ToList();

            var cities = await _db.Cities.OrderBy(c => c.Name).ToListAsync();

            var rawCategories = await _db.Categories
                .Select(c => new { c.Name, c.Slug, Count = c.Properties.Count() })
                .OrderBy(c => c.Name)
                .ToListAsync();

            var categories = rawCategories.Select(c => new CategoryCardViewModel
            {
                Name = c.Name,
                Slug = c.Slug,
                PropertyCount = c.Count,
                ImageUrl = _categoryImages.TryGetValue(c.Slug ?? c.Name, out var img)
                    ? img
                    : _defaultCategoryImage
            }).ToList();

            var vm = new HomeIndexViewModel
            {
                FeaturedProperties = featured,
                TopAgents = topAgents,
                Statistics = new HomeStatisticsViewModel
                {
                    TotalPropertiesCount = totalProperties,
                    TotalAgentsCount = totalAgents,
                    PropertiesSoldCount = totalProperties,
                    HappyClientsCount = totalEnquiries,
                    CitiesCount = totalCities
                },
                Cities = cities,
                Categories = categories
            };

            ViewBag.ActivePage = "Home";
            return View(vm);
        }

        public IActionResult News()
        {
            ViewBag.ActivePage = "News";
            return View();
        }

        public IActionResult About()
        {
            ViewBag.ActivePage = "About";
            return View();
        }

        public IActionResult Faq()
        {
            ViewBag.ActivePage = "Faq";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new RealtorsPortal.Models.ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
