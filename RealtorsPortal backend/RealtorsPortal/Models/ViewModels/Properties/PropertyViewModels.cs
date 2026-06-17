using RealtorsPortal.Models.ViewModels.Shared;

namespace RealtorsPortal.Models.ViewModels.Properties
{
    /// <summary>
    /// ViewModel for Properties/Index (listing page).
    /// Displays paginated property list with optional filters.
    /// Data Sources:
    ///   - IPropertyService.GetPropertiesAsync(filter, page, pageSize)
    ///   - IPropertyService.GetFilterOptionsAsync() [for dropdowns]
    /// Controller Action: PropertiesController.Index(PropertyFilterViewModel, int page)
    /// </summary>
    public class PropertyListViewModel
    {
        public List<PropertyCardViewModel> Properties { get; set; } = new();
        public PropertyFilterViewModel Filter { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }

    /// <summary>
    /// Filter criteria for property search/listing.
    /// Used in: Properties/Index, Advanced Search
    /// </summary>
    public class PropertyFilterViewModel
    {
        public string? Keyword { get; set; }
        public string? Location { get; set; } // area name or city
        public string? PropertyType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinBedrooms { get; set; }
        public int? MinBathrooms { get; set; }
        public decimal? MinArea { get; set; }
        public string? SortBy { get; set; } = "Newest"; // "Newest", "PriceAsc", "PriceDesc", "FeaturedFirst"

        // Dropdown options (populated by controller from service)
        public List<FilterOptionViewModel> LocationOptions { get; set; } = new();
        public List<FilterOptionViewModel> PropertyTypeOptions { get; set; } = new();
    }

    public class FilterOptionViewModel
    {
        public string Value { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int Count { get; set; } // number of properties with this filter value
    }

    /// <summary>
    /// ViewModel for Properties/Details (single property view).
    /// Displays: full details, gallery, agent card, similar properties, enquiry form
    /// Data Sources:
    ///   - IPropertyService.GetPropertyByIdAsync(id)
    ///   - IPropertyService.GetSimilarPropertiesAsync(propertyId, count)
    ///   - IAgentService.GetAgentByIdAsync(agentId) [if agent assigned]
    /// Controller Action: PropertiesController.Details(int id)
    /// </summary>
    public class PropertyDetailsViewModel
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

        // Gallery
        public List<PropertyImageViewModel> ImageGallery { get; set; } = new();

        // Agent info
        public AgentCardViewModel? ListingAgent { get; set; }

        // Related properties
        public List<PropertyCardViewModel> SimilarProperties { get; set; } = new();

        // Enquiry form for this property
        public PropertyEnquiryViewModel EnquiryForm { get; set; } = new();
    }

    public class PropertyImageViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; } = null!;
        public string? Caption { get; set; }
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
    }

    /// <summary>
    /// Form for property enquiry/lead submission.
    /// Submitted from: Property Details page
    /// Target Service Action: IContactService.SubmitPropertyEnquiryAsync()
    /// </summary>
    public class PropertyEnquiryViewModel
    {
        public int PropertyId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// ViewModel for Properties/AdvancedSearch (advanced search page).
    /// Extends PropertyFilterViewModel with additional fields and result display.
    /// Data Sources:
    ///   - IPropertyService.SearchPropertiesAsync(filter, page, pageSize)
    ///   - IPropertyService.GetFilterOptionsAsync()
    /// Controller Action: PropertiesController.AdvancedSearch(AdvancedSearchViewModel, int page)
    /// </summary>
    public class AdvancedSearchViewModel
    {
        // Filters
        public PropertyFilterViewModel Filters { get; set; } = new();

        // Results
        public List<PropertyCardViewModel> Results { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();

        // UI state
        public bool HasSearched { get; set; }
        public int? ResultsCount { get; set; }
    }
}
