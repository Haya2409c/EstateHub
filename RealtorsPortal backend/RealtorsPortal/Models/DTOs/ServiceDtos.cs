namespace RealtorsPortal.Models.DTOs
{
    /// <summary>
    /// DTO for property filter criteria passed from Controller to Service.
    /// Service converts this to Repository-level queries.
    /// </summary>
    public class PropertyFilterDto
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; }
        public string? PropertyType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinBedrooms { get; set; }
        public int? MinBathrooms { get; set; }
        public decimal? MinArea { get; set; }
        public string? SortBy { get; set; }
    }

    /// <summary>
    /// DTO for paginated property query results.
    /// Returned by service for controller to map into PropertyListViewModel.
    /// </summary>
    public class PropertyResultDto
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string Title { get; set; } = null!;
        public decimal Price { get; set; }
        public string? PropertyType { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public decimal? AreaSqft { get; set; }
        public string? Address { get; set; }
        public string? ThumbnailImageUrl { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime ListedDate { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public int? AreaId { get; set; }
        public string? AreaName { get; set; }
        public string? CityName { get; set; }
    }

    /// <summary>
    /// DTO for single property details (with related data).
    /// Includes images, agent, similar properties.
    /// </summary>
    public class PropertyDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? PropertyType { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public decimal? AreaSqft { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime ListedDate { get; set; }

        // Related data
        public List<PropertyImageDto> Images { get; set; } = new();
        public AgentDto? Agent { get; set; }
        public List<PropertyResultDto> SimilarProperties { get; set; } = new();
    }

    public class PropertyImageDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = null!;
        public string? Caption { get; set; }
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
    }

    /// <summary>
    /// DTO for filter options (distinct values used in dropdowns).
    /// </summary>
    public class FilterOptionsDto
    {
        public List<FilterOptionDto> Locations { get; set; } = new();
        public List<FilterOptionDto> PropertyTypes { get; set; } = new();
    }

    public class FilterOptionDto
    {
        public string Value { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int Count { get; set; }
    }

    /// <summary>
    /// DTO for agent card/listing.
    /// </summary>
    public class AgentDto
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Bio { get; set; }
        public int? YearsExperience { get; set; }
        public double? Rating { get; set; }
        public int PropertiesCount { get; set; }
    }

    /// <summary>
    /// DTO for agent profile (with properties and additional details).
    /// </summary>
    public class AgentProfileDto
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Bio { get; set; }
        public int? YearsExperience { get; set; }
        public double? Rating { get; set; }

        // Agent's properties
        public List<PropertyResultDto> ListedProperties { get; set; } = new();
    }

    /// <summary>
    /// DTO for news article card/listing.
    /// </summary>
    public class NewsArticleDto
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
    /// DTO for news article details (with related articles).
    /// </summary>
    public class NewsArticleDetailsDto
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
        public List<NewsArticleDto> RelatedArticles { get; set; } = new();
    }

    /// <summary>
    /// DTO for contact form submission (general enquiry).
    /// </summary>
    public class ContactMessageDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string Subject { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO for property enquiry submission.
    /// Extends ContactMessageDto with property context.
    /// </summary>
    public class PropertyEnquiryDto : ContactMessageDto
    {
        public int PropertyId { get; set; }
    }

    /// <summary>
    /// DTO for agent enquiry submission.
    /// Extends ContactMessageDto with agent context.
    /// </summary>
    public class AgentEnquiryDto : ContactMessageDto
    {
        public int AgentId { get; set; }
    }

    /// <summary>
    /// DTO for office location.
    /// </summary>
    public class OfficeLocationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Hours { get; set; }
    }

    /// <summary>
    /// DTO for home page statistics.
    /// </summary>
    public class StatisticsDto
    {
        public int TotalProperties { get; set; }
        public int TotalAgents { get; set; }
        public int PropertiesSold { get; set; }
        public int HappyClients { get; set; }
    }
}
