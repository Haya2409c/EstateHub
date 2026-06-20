using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtorsPortal.Data;
using RealtorsPortal.Models.ViewModels.News;
using RealtorsPortal.Models.ViewModels.Shared;

namespace RealtorsPortal.Controllers
{
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<NewsController> _logger;

        public NewsController(ApplicationDbContext db, ILogger<NewsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: /News
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 9;

            var total = await _db.NewsArticles.CountAsync();
            var articles = await _db.NewsArticles
                .OrderByDescending(n => n.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var recent = await _db.NewsArticles
                .OrderByDescending(n => n.PublishedAt)
                .Take(5)
                .Select(n => new NewsArticleCardViewModel
                {
                    Id           = n.Id,
                    Slug         = n.Slug,
                    Title        = n.Title,
                    ThumbnailUrl = n.ThumbnailUrl,
                    PublishedAt  = n.PublishedAt
                })
                .ToListAsync();

            var vm = new NewsListViewModel
            {
                Articles = articles.Select(n => new NewsArticleCardViewModel
                {
                    Id           = n.Id,
                    Slug         = n.Slug,
                    Title        = n.Title,
                    Summary      = n.Summary,
                    ThumbnailUrl = n.ThumbnailUrl,
                    PublishedAt  = n.PublishedAt,
                    Author       = n.Author
                }).ToList(),
                Pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    TotalPages  = (int)Math.Ceiling(total / (double)pageSize),
                    TotalItems  = total,
                    PageSize    = pageSize
                }
            };

            ViewBag.ActivePage    = "News";
            ViewBag.RecentArticles = recent;
            return View(vm);
        }

        // GET: /News/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var article = await _db.NewsArticles.FindAsync(id);
            if (article == null) return NotFound();

            var related = await _db.NewsArticles
                .Where(n => n.Id != id)
                .OrderByDescending(n => n.PublishedAt)
                .Take(3)
                .Select(n => new NewsArticleCardViewModel
                {
                    Id           = n.Id,
                    Slug         = n.Slug,
                    Title        = n.Title,
                    Summary      = n.Summary,
                    ThumbnailUrl = n.ThumbnailUrl,
                    PublishedAt  = n.PublishedAt,
                    Author       = n.Author
                })
                .ToListAsync();

            var vm = new NewsArticleDetailsViewModel
            {
                Id              = article.Id,
                Slug            = article.Slug,
                Title           = article.Title,
                Summary         = article.Summary,
                Content         = article.Content,
                Author          = article.Author,
                PublishedAt     = article.PublishedAt,
                ThumbnailUrl    = article.ThumbnailUrl,
                RelatedArticles = related
            };

            ViewBag.ActivePage = "News";
            return View(vm);
        }
    }
}
