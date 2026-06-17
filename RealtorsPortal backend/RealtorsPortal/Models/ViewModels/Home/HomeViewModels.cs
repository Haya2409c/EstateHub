using RealtorsPortal.Models.ViewModels.Shared;

namespace RealtorsPortal.Models.ViewModels.Home
{
    /// <summary>
    /// ViewModel for Home/Index page.
    /// Displays: featured properties, top agents, hero search widget, stats banner
    /// Data Sources:
    ///   - IPropertyService.GetFeaturedPropertiesAsync(count)
    ///   - IAgentService.GetTopAgentsAsync(count)
    ///   - IPropertyService.GetStatisticsAsync()
    /// Controller Action: HomeController.Index()
    /// </summary>
    public class HomeIndexViewModel
    {
        public List<PropertyCardViewModel> FeaturedProperties { get; set; } = new();
        public List<AgentCardViewModel> TopAgents { get; set; } = new();
        public HomeStatisticsViewModel Statistics { get; set; } = new();
        public PropertySearchFormViewModel QuickSearch { get; set; } = new();
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
