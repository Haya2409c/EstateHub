using RealtorsPortal.Models.DTOs;

namespace RealtorsPortal.Services.Interfaces
{
    /// <summary>
    /// Service interface for property operations.
    /// Responsibilities:
    ///   - Property queries (list, details, search, filters)
    ///   - Featured properties
    ///   - Statistics
    /// Data Flow: Repository -> DTO -> Service -> Controller -> ViewModel
    /// </summary>
    public interface IPropertyService
    {
        /// <summary>
        /// Get paginated properties with optional filters.
        /// Used by: Properties/Index
        /// </summary>
        Task<(List<PropertyResultDto> properties, int totalCount)> GetPropertiesAsync(
            PropertyFilterDto filter, 
            int page = 1, 
            int pageSize = 12);

        /// <summary>
        /// Get featured properties (limited count).
        /// Used by: Home/Index
        /// </summary>
        Task<List<PropertyResultDto>> GetFeaturedPropertiesAsync(int count = 6);

        /// <summary>
        /// Get single property with all related data (images, agent, similar).
        /// Used by: Properties/Details
        /// </summary>
        Task<PropertyDetailsDto?> GetPropertyByIdAsync(int id);

        /// <summary>
        /// Get similar/related properties for display on detail page.
        /// Used by: Properties/Details
        /// </summary>
        Task<List<PropertyResultDto>> GetSimilarPropertiesAsync(int propertyId, int count = 6);

        /// <summary>
        /// Get distinct filter options (for dropdowns).
        /// Used by: Properties/Index, AdvancedSearch (to populate form)
        /// </summary>
        Task<FilterOptionsDto> GetFilterOptionsAsync();

        /// <summary>
        /// Get home page statistics.
        /// Used by: Home/Index
        /// </summary>
        Task<StatisticsDto> GetStatisticsAsync();
    }

    /// <summary>
    /// Service interface for agent operations.
    /// Responsibilities:
    ///   - Agent queries (list, profile)
    ///   - Top agents
    ///   - Agent properties
    /// Data Flow: Repository -> DTO -> Service -> Controller -> ViewModel
    /// </summary>
    public interface IAgentService
    {
        /// <summary>
        /// Get paginated list of all agents with optional filters.
        /// Used by: Agents/Index
        /// </summary>
        Task<(List<AgentDto> agents, int totalCount)> GetAllAgentsAsync(
            AgentFilterDto? filter = null, 
            int page = 1, 
            int pageSize = 12);

        /// <summary>
        /// Get top-rated agents (limited count).
        /// Used by: Home/Index
        /// </summary>
        Task<List<AgentDto>> GetTopAgentsAsync(int count = 6);

        /// <summary>
        /// Get agent profile by slug (e.g., "hifza", "tayyaba").
        /// Used by: Agents/Profile
        /// </summary>
        Task<AgentProfileDto?> GetAgentBySlugAsync(string slug);

        /// <summary>
        /// Get agent by ID (used internally by other services).
        /// </summary>
        Task<AgentDto?> GetAgentByIdAsync(int id);

        /// <summary>
        /// Get paginated properties listed by specific agent.
        /// Used by: Agents/Profile (to display agent's listings)
        /// </summary>
        Task<(List<PropertyResultDto> properties, int totalCount)> GetPropertiesByAgentAsync(
            int agentId, 
            int page = 1, 
            int pageSize = 12);
    }

    /// <summary>
    /// Filter DTO for agent search (passed from controller to service).
    /// </summary>
    public class AgentFilterDto
    {
        public string? Specialization { get; set; }
        public string? Location { get; set; }
        public string? SortBy { get; set; }
    }

    /// <summary>
    /// Service interface for news operations.
    /// Responsibilities:
    ///   - News article queries (list, details)
    ///   - Related articles
    /// Data Flow: Repository -> DTO -> Service -> Controller -> ViewModel
    /// </summary>
    public interface INewsService
    {
        /// <summary>
        /// Get paginated list of news articles.
        /// Used by: News/Index
        /// </summary>
        Task<(List<NewsArticleDto> articles, int totalCount)> GetArticlesAsync(
            int page = 1, 
            int pageSize = 12);

        /// <summary>
        /// Get single news article with related articles.
        /// Used by: News/Details
        /// </summary>
        Task<NewsArticleDetailsDto?> GetArticleByIdAsync(int id);

        /// <summary>
        /// Get related articles for display on detail page.
        /// Used by: News/Details
        /// </summary>
        Task<List<NewsArticleDto>> GetRelatedArticlesAsync(int articleId, int count = 3);
    }

    /// <summary>
    /// Service interface for contact/enquiry operations.
    /// Responsibilities:
    ///   - Contact form submission
    ///   - Property enquiry submission
    ///   - Agent enquiry submission
    ///   - Office locations
    /// Data Flow: ViewModel -> DTO -> Service -> Repository/Email
    /// </summary>
    public interface IContactService
    {
        /// <summary>
        /// Submit general contact form message.
        /// Used by: Contact/Index (POST)
        /// </summary>
        Task SubmitGeneralEnquiryAsync(ContactMessageDto message);

        /// <summary>
        /// Submit property enquiry/lead.
        /// Used by: Properties/Details (property enquiry form POST)
        /// </summary>
        Task SubmitPropertyEnquiryAsync(PropertyEnquiryDto enquiry);

        /// <summary>
        /// Submit agent enquiry/lead.
        /// Used by: Agents/Profile (agent contact form POST)
        /// </summary>
        Task SubmitAgentEnquiryAsync(AgentEnquiryDto enquiry);

        /// <summary>
        /// Get office locations for display on contact page.
        /// Used by: Contact/Index
        /// </summary>
        Task<List<OfficeLocationDto>> GetOfficeLocationsAsync();
    }
}
