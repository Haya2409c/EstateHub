using RealtorsPortal.Models.ViewModels.Shared;

namespace RealtorsPortal.Models.ViewModels.News
{
    /// <summary>
    /// ViewModel for News/Index (news listing page).
    /// Displays paginated news articles.
    /// Data Sources:
    ///   - INewsService.GetArticlesAsync(page, pageSize)
    /// Controller Action: NewsController.Index(int page)
    /// </summary>
    public class NewsListViewModel
    {
        public List<NewsArticleCardViewModel> Articles { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }

    /// <summary>
    /// Card view for news article in listing.
    /// </summary>
    public class NewsArticleCardViewModel
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string Title { get; set; } = null!;
        public string? Summary { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime PublishedAt { get; set; }
        public string? Author { get; set; }
    }

    /// <summary>
    /// ViewModel for News/Details (single article view).
    /// Displays: full article content, author info, publish date, related articles
    /// Data Sources:
    ///   - INewsService.GetArticleByIdAsync(id)
    ///   - INewsService.GetRelatedArticlesAsync(articleId, count)
    /// Controller Action: NewsController.Details(int id)
    /// </summary>
    public class NewsArticleDetailsViewModel
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string Title { get; set; } = null!;
        public string? Summary { get; set; }
        public string Content { get; set; } = null!;
        public string? Author { get; set; }
        public DateTime PublishedAt { get; set; }
        public string? ThumbnailUrl { get; set; }

        // Related articles
        public List<NewsArticleCardViewModel> RelatedArticles { get; set; } = new();
    }
}
