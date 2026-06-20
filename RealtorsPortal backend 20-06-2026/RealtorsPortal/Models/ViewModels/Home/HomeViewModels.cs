using RealtorsPortal.Models.Entities;
using RealtorsPortal.Models.ViewModels.Shared;

namespace RealtorsPortal.Models.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public List<PropertyCardViewModel> FeaturedProperties { get; set; } = new();
        public List<AgentCardViewModel> TopAgents { get; set; } = new();
        public HomeStatisticsViewModel Statistics { get; set; } = new();
        public PropertySearchFormViewModel QuickSearch { get; set; } = new();
        public List<City> Cities { get; set; } = new();
        public List<CategoryCardViewModel> Categories { get; set; } = new();
    }

    public class CategoryCardViewModel
    {
        public string Name { get; set; } = "";
        public string? Slug { get; set; }
        public string ImageUrl { get; set; } = "";
        public int PropertyCount { get; set; }
    }

    /// <summary>
    /// Statistics/metrics displayed on home page (banner section)
    /// </summary>
    public class HomeStatisticsViewModel
    {
        public int TotalPropertiesCount { get; set; }
        public int TotalAgentsCount { get; set; }
        public int PropertiesSoldCount { get; set; }
        public int HappyClientsCount { get; set; }
        public int CitiesCount { get; set; }
    }

    /// <summary>
    /// Hero search widget on home page.
    /// Allows quick filtering by location and price range.
    /// </summary>
    public class PropertySearchFormViewModel
    {
        public string? Location { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? PropertyType { get; set; }
    }
}
