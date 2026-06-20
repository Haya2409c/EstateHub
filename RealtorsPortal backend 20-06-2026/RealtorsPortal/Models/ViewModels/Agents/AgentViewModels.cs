using RealtorsPortal.Models.ViewModels.Shared;

namespace RealtorsPortal.Models.ViewModels.Agents
{
    /// <summary>
    /// ViewModel for Agents/Index (agent listing page).
    /// Displays paginated agent list with optional filters.
    /// Data Sources:
    ///   - IAgentService.GetAllAgentsAsync(filter, page, pageSize)
    /// Controller Action: AgentsController.Index(string? specialization, string? location, int page)
    /// </summary>
    public class AgentCardViewModel
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PhotoUrl { get; set; }
        public int ListingCount { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class AgentListViewModel
    {
        public List<AgentCardViewModel> Agents { get; set; } = new();
        public AgentFilterViewModel Filter { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
    }

    /// <summary>
    /// Filter criteria for agent search/listing.
    /// </summary>
    public class AgentFilterViewModel
    {
        public string? Specialization { get; set; } // e.g., "Residential", "Commercial"
        public string? Location { get; set; }
        public string? SortBy { get; set; } = "Rating"; // "Rating", "Experience", "Name"
    }

    /// <summary>
    /// ViewModel for Agents/Profile (agent detail page).
    /// Displays: agent profile, listed properties, testimonials (if available), contact form
    /// Data Sources:
    ///   - IAgentService.GetAgentBySlugAsync(slug)
    ///   - IAgentService.GetPropertiesByAgentAsync(agentId, page, pageSize)
    /// Controller Action: AgentsController.Profile(string id) [where id is slug: "hifza", "tayyaba", etc.]
    /// </summary>
    public class AgentProfileViewModel
    {
        // Agent info
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Bio { get; set; }
        public int? YearsExperience { get; set; }
        public double? Rating { get; set; }

        // Agent's listings
        public List<PropertyCardViewModel> ListedProperties { get; set; } = new();
        public PaginationViewModel PropertiesPagination { get; set; } = new();

        // Contact form
        public AgentContactFormViewModel ContactForm { get; set; } = new();

        // Testimonials (optional, for future use)
        public List<TestimonialViewModel> Testimonials { get; set; } = new();
    }

    /// <summary>
    /// Form for contacting an agent directly.
    /// Submitted from: Agent Profile page
    /// Target Service Action: IContactService.SubmitAgentEnquiryAsync()
    /// </summary>
    public class AgentContactFormViewModel
    {
        public int AgentId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Testimonial view model (for future use if testimonials feature is added)
    /// </summary>
    public class TestimonialViewModel
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public double Rating { get; set; }
        public DateTime DateSubmitted { get; set; }
    }
}
