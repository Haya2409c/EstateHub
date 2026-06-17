/// <summary>
/// REALTORS PORTAL - PUBLIC WEBSITE ARCHITECTURE
/// ASP.NET Core MVC (.NET 8) - Entity Framework Core SQL Server
/// 
/// This document provides a comprehensive overview of the data flow and architecture
/// for the public website pages. All components are placeholders ready for EF Core implementation.
/// 
/// ============================================================================
/// 1. LAYERED ARCHITECTURE OVERVIEW
/// ============================================================================
/// 
/// Request Flow:
/// View (Razor) 
///   ↓ 
/// Controller (receives request, orchestrates)
///   ↓ 
/// Service (business logic, data transformation)
///   ↓ 
/// Repository (data access abstraction)
///   ↓ 
/// ApplicationDbContext (EF Core entity queries)
///   ↓ 
/// SQL Server Database
/// 
/// Response Flow (opposite direction):
/// Database Entity → Repository.GetXAsync() 
///   ↓
/// Entity → Service.MapToDto() → Dto
///   ↓
/// Dto → Controller.MapToViewModel() → ViewModel
///   ↓
/// ViewModel → View (Razor renders HTML)
/// 
/// ============================================================================
/// 2. PAGE-BY-PAGE ARCHITECTURE
/// ============================================================================
/// 
/// PAGE: Home / Index
/// ================
/// Route: GET /Home or /
/// 
/// ViewModel: HomeIndexViewModel
///   - List<PropertyCardViewModel> FeaturedProperties
///   - List<AgentCardViewModel> TopAgents
///   - HomeStatisticsViewModel Statistics
///   - PropertySearchFormViewModel QuickSearch
/// 
/// Controller Action: HomeController.Index()
///   1. Call IPropertyService.GetFeaturedPropertiesAsync(6)
///   2. Call IAgentService.GetTopAgentsAsync(6)
///   3. Call IPropertyService.GetStatisticsAsync()
///   4. Map Dto results to ViewModels
///   5. Return View(HomeIndexViewModel)
/// 
/// Service Method: IPropertyService.GetFeaturedPropertiesAsync(count)
///   1. Call IPropertyRepository.GetFeaturedAsync(count)
///   2. Include: PropertyImages (primary), Category, Agent, Area
///   3. Order by: ListedDate DESC
///   4. Map Entity → PropertyResultDto
///   5. Return List<PropertyResultDto>
/// 
/// Repository Method: IPropertyRepository.GetFeaturedAsync(count)
///   SQL Query:
///   SELECT TOP count p.*, c.Name as CategoryName, a.FullName as AgentName, ar.Name as AreaName, 
///          pi.Url as ThumbnailUrl
///   FROM Properties p
///   LEFT JOIN Categories c ON p.CategoryId = c.Id
///   LEFT JOIN Agents a ON p.AgentId = a.Id
///   LEFT JOIN Areas ar ON p.AreaId = ar.Id
///   LEFT JOIN PropertyImages pi ON p.Id = pi.PropertyId AND pi.IsPrimary = 1
///   WHERE p.IsFeatured = 1
///   ORDER BY p.ListedDate DESC
/// 
/// Entity Relationships:
///   - Property.CategoryId → Category.Id
///   - Property.AgentId → Agent.Id (nullable)
///   - Property.AreaId → Area.Id (nullable)
///   - Area.CityId → City.Id
///   - City.RegionId → Region.Id
///   - Region.CountryId → Country.Id
/// 
/// ---
/// 
/// PAGE: Properties / Index (Listing)
/// ==================================
/// Route: GET /Properties or /Properties/Index
/// Query Parameters: ?location=&propertyType=&minPrice=&maxPrice=&page=1
/// 
/// ViewModel: PropertyListViewModel
///   - List<PropertyCardViewModel> Properties
///   - PropertyFilterViewModel Filter
///   - PaginationViewModel Pagination
/// 
/// Controller Action: PropertiesController.Index(PropertyFilterViewModel filter, int page)
///   1. Create PropertyFilterDto from filter parameter
///   2. Call IPropertyService.GetPropertiesAsync(filterDto, page, 12)
///   3. Call IPropertyService.GetFilterOptionsAsync() for dropdowns
///   4. Map PropertyResultDto → PropertyCardViewModel
///   5. Map FilterOptionsDto → FilterOptionViewModel (for dropdowns)
///   6. Build PaginationViewModel (current page, total pages, etc.)
///   7. Return View(PropertyListViewModel)
/// 
/// Service Method: IPropertyService.GetPropertiesAsync(filter, page, pageSize)
///   1. Call IPropertyRepository.SearchAsync(filter params, page, pageSize)
///   2. Apply filtering: keyword (title/description), location, price range, bedrooms, bathrooms, area
///   3. Apply sorting: newest, price asc/desc, featured first
///   4. Apply pagination: SKIP (page-1)*pageSize, TAKE pageSize
///   5. Count total before pagination
///   6. Map Entity list → PropertyResultDto list
///   7. Return (properties, totalCount)
/// 
/// Repository Method: IPropertyRepository.SearchAsync(...)
///   SQL Query (dynamic WHERE based on filters):
///   SELECT p.*, c.Name, a.FullName, ar.Name, pi.Url, COUNT(*) OVER() as TotalCount
///   FROM Properties p
///   LEFT JOIN Categories c ON p.CategoryId = c.Id
///   LEFT JOIN Agents a ON p.AgentId = a.Id
///   LEFT JOIN Areas ar ON p.AreaId = ar.Id
///   LEFT JOIN PropertyImages pi ON p.Id = pi.PropertyId AND pi.IsPrimary = 1
///   WHERE (p.Title LIKE @keyword OR p.Description LIKE @keyword)
///     AND (ar.Name = @location OR @location IS NULL)
///     AND (p.PropertyType = @type OR @type IS NULL)
///     AND (p.Price BETWEEN @minPrice AND @maxPrice)
///     AND (p.Bedrooms >= @minBeds OR @minBeds IS NULL)
///     ...
///   ORDER BY [SORT BY LOGIC]
///   OFFSET (@page-1)*@pageSize ROWS FETCH NEXT @pageSize ROWS ONLY
/// 
/// ---
/// 
/// PAGE: Property Details
/// ======================
/// Route: GET /Properties/Details/{id}
/// 
/// ViewModel: PropertyDetailsViewModel
///   - int Id, Title, Description, Price, PropertyType, Bedrooms, etc.
///   - List<PropertyImageViewModel> ImageGallery
///   - AgentCardViewModel ListingAgent
///   - List<PropertyCardViewModel> SimilarProperties
///   - PropertyEnquiryViewModel EnquiryForm
/// 
/// Controller Action: PropertiesController.Details(int id)
///   1. Call IPropertyService.GetPropertyByIdAsync(id)
///   2. If null, return NotFound()
///   3. Map PropertyDetailsDto → PropertyDetailsViewModel
///   4. Return View(viewModel)
/// 
/// Service Method: IPropertyService.GetPropertyByIdAsync(id)
///   1. Call IPropertyRepository.GetWithDetailsAsync(id)
///   2. Include: ALL PropertyImages (sorted), Category, Agent, Area, City, Region, Country
///   3. Call GetSimilarPropertiesAsync(id, 6) for related properties
///   4. Map Entity → PropertyDetailsDto (with images, agent, similar)
///   5. Return PropertyDetailsDto or null
/// 
/// Repository Method: IPropertyRepository.GetWithDetailsAsync(id)
///   SQL Query:
///   SELECT p.*, c.Name, a.*, ar.Name, ct.Name as CityName, pi.*
///   FROM Properties p
///   LEFT JOIN Categories c ON p.CategoryId = c.Id
///   LEFT JOIN Agents a ON p.AgentId = a.Id
///   LEFT JOIN Areas ar ON p.AreaId = ar.Id
///   LEFT JOIN Cities ct ON ar.CityId = ct.Id
///   LEFT JOIN PropertyImages pi ON p.Id = pi.PropertyId
///   WHERE p.Id = @id
///   ORDER BY pi.IsPrimary DESC, pi.SortOrder ASC
/// 
/// Enquiry Form Submission:
/// Route: POST /Properties/ContactAgent
/// 
/// ViewModel: PropertyEnquiryViewModel
///   - int PropertyId
///   - string FullName, Email, Phone
///   - string Message
/// 
/// Controller Action: PropertiesController.ContactAgent(PropertyEnquiryViewModel model)
///   1. Validate ModelState
///   2. Create PropertyEnquiryDto from model
///   3. Call IContactService.SubmitPropertyEnquiryAsync(dto)
///   4. Return JSON success or redirect
/// 
/// Service Method: IContactService.SubmitPropertyEnquiryAsync(dto)
///   1. Validate dto
///   2. Verify property exists
///   3. Get agent if property has one
///   4. Create ContactMessage entity
///   5. Call IContactRepository.SaveMessageAsync(message)
///   6. Call IEmailService.SendPropertyEnquiryEmailAsync() to agent/admin
///   7. Return success
/// 
/// ---
/// 
/// PAGE: Agents / Index (Listing)
/// ==============================
/// Route: GET /Agents or /Agents/Index
/// Query Parameters: ?specialization=&location=&page=1
/// 
/// ViewModel: AgentListViewModel
///   - List<AgentCardViewModel> Agents
///   - AgentFilterViewModel Filter
///   - PaginationViewModel Pagination
/// 
/// Controller Action: AgentsController.Index(string? specialization, string? location, int page)
///   1. Create AgentFilterDto from parameters
///   2. Call IAgentService.GetAllAgentsAsync(filter, page, 12)
///   3. Map AgentDto → AgentCardViewModel (include properties count)
///   4. Build PaginationViewModel
///   5. Return View(AgentListViewModel)
/// 
/// Service Method: IAgentService.GetAllAgentsAsync(filter, page, pageSize)
///   1. Call IAgentRepository.SearchAsync(filter, page, pageSize)
///   2. For each agent, count properties via IPropertyRepository.GetCountByAgentAsync()
///   3. Apply sorting: rating, experience, name
///   4. Apply pagination
///   5. Map Entity → AgentDto (with properties count)
///   6. Return (agents, totalCount)
/// 
/// ---
/// 
/// PAGE: Agent Profile
/// ====================
/// Route: GET /Agents/Profile/{id}
/// Where id = slug (e.g., "hifza", "tayyaba", "harmain")
/// 
/// ViewModel: AgentProfileViewModel
///   - int Id, string Slug, FullName, Email, Phone, PhotoUrl, Bio, YearsExperience, Rating
///   - List<PropertyCardViewModel> ListedProperties
///   - PaginationViewModel PropertiesPagination
///   - AgentContactFormViewModel ContactForm
///   - List<TestimonialViewModel> Testimonials (future)
/// 
/// Controller Action: AgentsController.Profile(string id)
///   1. Normalize id to lowercase
///   2. Call IAgentService.GetAgentBySlugAsync(id)
///   3. If null, return NotFound()
///   4. Get properties via GetPropertiesByAgentAsync(id, page 1, 12)
///   5. Map AgentProfileDto → AgentProfileViewModel
///   6. Determine view name based on slug (maintain UI): Profile_hifza, Profile_tayyaba, Profile_harmain
///   7. Return View(viewName, viewModel)
/// 
/// Service Method: IAgentService.GetAgentBySlugAsync(slug)
///   1. Call IAgentRepository.GetBySlugAsync(slug)
///   2. If null, return null
///   3. Call GetPropertiesByAgentAsync(agentId, 1, 12)
///   4. Map Entity → AgentProfileDto (with properties)
///   5. Return AgentProfileDto or null
/// 
/// Repository Method: IAgentRepository.GetBySlugAsync(slug)
///   SQL Query:
///   SELECT * FROM Agents WHERE Slug = @slug
/// 
/// Contact Form Submission:
/// Route: POST /Agents/ContactAgent
/// 
/// ViewModel: AgentContactFormViewModel
///   - int AgentId
///   - string FullName, Email, Phone
///   - string Message
/// 
/// Controller Action: AgentsController.ContactAgent(AgentContactFormViewModel model)
///   1. Validate ModelState
///   2. Create AgentEnquiryDto from model
///   3. Call IContactService.SubmitAgentEnquiryAsync(dto)
///   4. Return JSON success or redirect
/// 
/// ---
/// 
/// PAGE: News / Index (Listing)
/// ============================
/// Route: GET /News or /News/Index
/// Query Parameters: ?page=1
/// 
/// ViewModel: NewsListViewModel
///   - List<NewsArticleCardViewModel> Articles
///   - PaginationViewModel Pagination
/// 
/// Controller Action: NewsController.Index(int page)
///   1. Call INewsService.GetArticlesAsync(page, 12)
///   2. Map NewsArticleDto → NewsArticleCardViewModel
///   3. Build PaginationViewModel
///   4. Return View(NewsListViewModel)
/// 
/// Service Method: INewsService.GetArticlesAsync(page, pageSize)
///   1. Call INewsRepository.GetPagedAsync(page, pageSize)
///   2. Order by PublishedAt DESC
///   3. Apply pagination
///   4. Map Entity → NewsArticleDto
///   5. Return (articles, totalCount)
/// 
/// ---
/// 
/// PAGE: News Details
/// ===================
/// Route: GET /News/Details/{id}
/// 
/// ViewModel: NewsArticleDetailsViewModel
///   - int Id, string Title, Summary, Content, Author, PublishedAt, ThumbnailUrl
///   - List<NewsArticleCardViewModel> RelatedArticles
/// 
/// Controller Action: NewsController.Details(int id)
///   1. Call INewsService.GetArticleByIdAsync(id)
///   2. If null, return NotFound()
///   3. Map NewsArticleDetailsDto → NewsArticleDetailsViewModel
///   4. Return View(viewModel)
/// 
/// Service Method: INewsService.GetArticleByIdAsync(id)
///   1. Call INewsRepository.GetByIdAsync(id)
///   2. Call GetRelatedArticlesAsync(id, 3)
///   3. Map Entity → NewsArticleDetailsDto (with related articles)
///   4. Return NewsArticleDetailsDto or null
/// 
/// ---
/// 
/// PAGE: Contact / Index
/// ======================
/// Route: GET /Contact or /Contact/Index
/// 
/// ViewModel: ContactPageViewModel
///   - ContactFormViewModel Form
///   - List<OfficeLocationViewModel> Offices
/// 
/// Controller Action: ContactController.Index()
///   1. Call IContactService.GetOfficeLocationsAsync()
///   2. Map OfficeLocationDto → OfficeLocationViewModel
///   3. Return View(ContactPageViewModel)
/// 
/// Service Method: IContactService.GetOfficeLocationsAsync()
///   1. Call IContactRepository.GetOfficeLocationsAsync()
///   2. For now, return hardcoded list
///   3. Future: Query from database table
///   4. Map to OfficeLocationDto
///   5. Return List<OfficeLocationDto>
/// 
/// Form Submission:
/// Route: POST /Contact or /Contact/Index
/// 
/// ViewModel: ContactFormViewModel
///   - string FullName, Email, Phone, Subject, Message
/// 
/// Controller Action: ContactController.Index(ContactFormViewModel model)
///   1. Validate ModelState
///   2. Create ContactMessageDto from model
///   3. Call IContactService.SubmitGeneralEnquiryAsync(dto)
///   4. Set TempData success message
///   5. Redirect to Index (showing success)
/// 
/// Service Method: IContactService.SubmitGeneralEnquiryAsync(dto)
///   1. Validate dto
///   2. Create ContactMessage entity
///   3. Call IContactRepository.SaveMessageAsync(message)
///   4. Call IEmailService.SendContactNotificationAsync() to admin
///   5. Return success
/// 
/// ============================================================================
/// 3. ENTITY RELATIONSHIPS
/// ============================================================================
/// 
/// Geographic Hierarchy:
///   Country (1) → Many Regions
///   Region (1) → Many Cities
///   City (1) → Many Areas
///   Area (1) → Many Properties
/// 
/// Property Relationships:
///   Category (1) → Many Properties
///   Agent (1) → Many Properties
///   Property (1) → Many PropertyImages
/// 
/// Content Relationships:
///   News (standalone, no foreign keys)
///   ContactMessage (standalone, no foreign keys)
/// 
/// ============================================================================
/// 4. DEPENDENCY INJECTION (Program.cs)
/// ============================================================================
/// 
/// builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
/// builder.Services.AddScoped<IAgentRepository, AgentRepository>();
/// builder.Services.AddScoped<INewsRepository, NewsRepository>();
/// builder.Services.AddScoped<IContactRepository, ContactRepository>();
/// 
/// builder.Services.AddScoped<IPropertyService, PropertyService>();
/// builder.Services.AddScoped<IAgentService, AgentService>();
/// builder.Services.AddScoped<INewsService, NewsService>();
/// builder.Services.AddScoped<IContactService, ContactService>();
/// 
/// ============================================================================
/// 5. MAPPING STRATEGY
/// ============================================================================
/// 
/// Entity (from DB) → Dto (between Service and Controller) → ViewModel (to View)
/// 
/// Mapping Locations:
///   - Entity → Dto: Inside Service methods (manual mapping or AutoMapper)
///   - Dto → ViewModel: Inside Controller actions (manual mapping)
/// 
/// Benefits:
///   - Entities never leave Service layer (data security)
///   - DTOs act as contract between layers (versioning)
///   - ViewModels shaped for UI consumption (cleaner views)
///   - Easy to add new mappings without affecting existing layers
/// 
/// ============================================================================
/// 6. ERROR HANDLING STRATEGY
/// ============================================================================
/// 
/// Controllers:
///   - Wrap action logic in try-catch
///   - Log exceptions via ILogger
///   - Return NotFound() or Error view for user-facing errors
///   - Return BadRequest() for validation failures
/// 
/// Services:
///   - Validate input parameters
///   - Let exceptions bubble up to controller (or log and return default)
///   - Use Task-based async methods throughout
/// 
/// Repositories:
///   - Let DbContext exceptions bubble to service layer
///   - No business logic here (only data access)
/// 
/// ============================================================================
/// 7. PERFORMANCE CONSIDERATIONS
/// ============================================================================
/// 
/// EF Core Optimization:
///   - Use Include() for related entities (prevent N+1 queries)
///   - Use Select() to project only needed columns
///   - Use async methods (GetPropertiesAsync, not GetProperties)
///   - Add indexes on: CategoryId, AgentId, AreaId, PropertyType, Price, IsFeatured
/// 
/// Caching (future):
///   - Featured properties (rarely change)
///   - Agents list (rarely change)
///   - Filter options (distinct values)
///   - Consider IDistributedCache for multi-server scenarios
/// 
/// ============================================================================
/// 8. FUTURE ENHANCEMENTS
/// ============================================================================
/// 
/// Phase 2 - Seller Dashboard:
///   - Add Seller entity and tables: SellerSubscriptions, SellerListings
///   - Create SellerController with CRUD for listings
///   - Add ISellerService for seller-specific operations
/// 
/// Phase 3 - Agent Dashboard:
///   - Link Agent to aspnetuser (identity)
///   - Add AgentMetrics, AgentCommissions tables
///   - Create AgentDashboardController for leads, commissions, stats
/// 
/// Phase 4 - Admin Panel:
///   - Create AdminController with full CRUD for all entities
///   - Add AdminUser role-based authorization
///   - Create AuditLog table for tracking changes
///   - Add SiteSettings key-value table for site-wide configuration
/// 
/// ============================================================================
/// </summary>
namespace RealtorsPortal.Documentation
{
    // This file is for documentation purposes only.
}
