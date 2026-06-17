using Microsoft.AspNetCore.Mvc;
using RealtorsPortal.Models.ViewModels.News;
using RealtorsPortal.Services.Interfaces;

namespace RealtorsPortal.Controllers
{
    /// <summary>
    /// News Controller
    /// Handles news article listing and detail pages.
    /// 
    /// Data Flow:
    /// 1. Action calls INewsService
    /// 2. Service queries INewsRepository, returns DTOs
    /// 3. Controller maps DTOs to ViewModels
    /// 4. View renders with dynamic database data
    /// 
    /// Dependencies:
    ///   - INewsService: articles, article details, related articles
    /// </summary>
    public class NewsController : Controller
    {
        // TODO: Inject INewsService, ILogger

        public NewsController()
        {
            // Constructor placeholder
        }

        /// <summary>
        /// GET: /News or /News/Index
        /// News listing page with pagination.
        /// </summary>
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var viewModel = new NewsListViewModel();

                // TODO: Implement
                // Steps:
                //   1. Call INewsService.GetArticlesAsync(page, 12)
                //   2. Map NewsArticleDto list to NewsArticleCardViewModel list
                //   3. Build PaginationViewModel
                //   4. Return View(viewModel)

                ViewBag.ActivePage = "News";
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// GET: /News/Details/{id}
        /// Single news article detail view with related articles.
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // TODO: Implement
                // Steps:
                //   1. Call INewsService.GetArticleByIdAsync(id)
                //   2. If null, return NotFound()
                //   3. Map NewsArticleDetailsDto to NewsArticleDetailsViewModel
                //   4. Return View(viewModel)

                var viewModel = new NewsArticleDetailsViewModel();
                ViewBag.ActivePage = "News";
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
