namespace RealtorsPortal.Models.ViewModels.Shared
{
    /// <summary>
    /// Pagination metadata for list views.
    /// Used across all paginated pages (Properties, Agents, News, etc.)
    /// </summary>
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 12;

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int PreviousPage => CurrentPage - 1;
        public int NextPage => CurrentPage + 1;
    }

    /// <summary>
    /// Reusable card view for property display in lists and grids.
    /// Used in: Home (featured), Properties list, Agent profile, Property details (similar)
    /// </summary>
    public class PropertyCardViewModel
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
        public string? ThumbnailUrl { get; set; } // primary image
        public bool IsFeatured { get; set; }
        public DateTime ListedDate { get; set; }

        public string? Status { get; set; } // "buy" / "rent" / "sell"

        // Agent reference (if applicable)
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }

        // Location reference
        public int? AreaId { get; set; }
        public string? AreaName { get; set; }
        public string? CityName { get; set; }
    }

    /// <summary>
    /// Reusable card view for agent display in lists and grids.
    /// Used in: Home (top agents), Agents list, Property details (listing agent)
    /// </summary>
    public class AgentCardViewModel
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
}
