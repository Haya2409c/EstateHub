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

        // GET: /
        public async Task<IActionResult> Index()
        {
            var recentProperties = await _db.Properties
                .Include(p => p.Images)
                .Include(p => p.Category)
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
                    ?? p.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url
                    ?? "/assets/images/new-property-1.jpg",
                ListedDate = p.ListedDate
            }).ToList();

            var totalProperties = await _db.Properties.CountAsync();

            var vm = new HomeIndexViewModel
            {
                FeaturedProperties = featured,
                Statistics = new HomeStatisticsViewModel
                {
                    TotalPropertiesCount = totalProperties,
                    TotalAgentsCount = 0,
                    PropertiesSoldCount = 0,
                    HappyClientsCount = 0
                }
            };

            ViewBag.ActivePage = "Home";
            return View(vm);
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
