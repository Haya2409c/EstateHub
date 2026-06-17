# Realtors Portal - Quick Implementation Reference

## 🎯 For Developers Starting Implementation

### Step-by-Step: Implement One Feature End-to-End

#### Example: Property Listing with Filters (Properties/Index)

##### 1. **Database Query** (Repository)
**File:** `Repositories/Implementations/PublicWebsiteRepositories.cs`

```csharp
// PropertyRepository.SearchAsync() - Implement this
public async Task<(List<Property> properties, int totalCount)> SearchAsync(
    string? keyword, string? location, string? propertyType,
    decimal? minPrice, decimal? maxPrice,
    int? minBedrooms, int? minBathrooms, decimal? minArea,
    string? sortBy, int page, int pageSize)
{
    // TODO: Fill in with:
    var query = _context.Properties
        .AsQueryable()
        .Include(p => p.Category)
        .Include(p => p.Agent)
        .Include(p => p.Area)
        .ThenInclude(a => a.City)
        .ThenInclude(c => c.Region);

    // Apply filters...
    if (!string.IsNullOrEmpty(keyword))
        query = query.Where(p => p.Title.Contains(keyword) || p.Description.Contains(keyword));

    if (!string.IsNullOrEmpty(location))
        query = query.Where(p => p.Area.Name == location);

    // ... continue for all filters

    int totalCount = await query.CountAsync();

    var results = await query
        .OrderByDescending(p => p.ListedDate) // Apply sorting based on sortBy param
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (results, totalCount);
}
```

##### 2. **Service Layer** (Maps Entity → DTO, Calls Repository)
**File:** `Services/Implementations/PublicWebsiteServices.cs`

```csharp
// PropertyService.GetPropertiesAsync() - Implement this
public async Task<(List<PropertyResultDto> properties, int totalCount)> GetPropertiesAsync(
    PropertyFilterDto filter, int page = 1, int pageSize = 12)
{
    // TODO: Fill in with:
    var (entities, totalCount) = await _propertyRepository.SearchAsync(
        keyword: filter.Keyword,
        location: filter.Location,
        propertyType: filter.PropertyType,
        minPrice: filter.MinPrice,
        maxPrice: filter.MaxPrice,
        minBedrooms: filter.MinBedrooms,
        minBathrooms: filter.MinBathrooms,
        minArea: filter.MinArea,
        sortBy: filter.SortBy,
        page: page,
        pageSize: pageSize
    );

    var dtos = entities.Select(p => new PropertyResultDto
    {
        Id = p.Id,
        Slug = p.Slug,
        Title = p.Title,
        Price = p.Price,
        PropertyType = p.PropertyType,
        Bedrooms = p.Bedrooms,
        Bathrooms = p.Bathrooms,
        AreaSqft = p.AreaSqft,
        Address = p.Address,
        ThumbnailImageUrl = p.Images?.FirstOrDefault(img => img.IsPrimary)?.Url,
        IsFeatured = p.IsFeatured,
        ListedDate = p.ListedDate,
        AgentId = p.AgentId,
        AgentName = p.Agent?.FullName,
        AreaId = p.AreaId,
        AreaName = p.Area?.Name,
        CityName = p.Area?.City?.Name
    }).ToList();

    return (dtos, totalCount);
}
```

##### 3. **Controller Action** (Maps DTO → ViewModel, Calls Service)
**File:** `Controllers/PropertiesController.cs`

```csharp
// Index() action - Already structured, fill in with:
public async Task<IActionResult> Index(PropertyFilterViewModel? filter, int page = 1)
{
    try
    {
        var viewModel = new PropertyListViewModel();
        filter ??= new PropertyFilterViewModel();

        // Create DTO from ViewModel filter
        var filterDto = new PropertyFilterDto
        {
            Keyword = filter.Keyword,
            Location = filter.Location,
            PropertyType = filter.PropertyType,
            MinPrice = filter.MinPrice,
            MaxPrice = filter.MaxPrice,
            MinBedrooms = filter.MinBedrooms,
            MinBathrooms = filter.MinBathrooms,
            MinArea = filter.MinArea,
            SortBy = filter.SortBy ?? "Newest"
        };

        // Call service
        var (propertyDtos, totalCount) = await _propertyService.GetPropertiesAsync(filterDto, page, 12);

        // Get filter options for dropdowns
        var filterOptions = await _propertyService.GetFilterOptionsAsync();

        // Map DTOs to ViewModels
        viewModel.Properties = propertyDtos.Select(dto => new PropertyCardViewModel
        {
            Id = dto.Id,
            Slug = dto.Slug,
            Title = dto.Title,
            Price = dto.Price,
            PropertyType = dto.PropertyType,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            AreaSqft = dto.AreaSqft,
            Address = dto.Address,
            ThumbnailUrl = dto.ThumbnailImageUrl,
            IsFeatured = dto.IsFeatured,
            ListedDate = dto.ListedDate,
            AgentId = dto.AgentId,
            AgentName = dto.AgentName,
            AreaId = dto.AreaId,
            AreaName = dto.AreaName,
            CityName = dto.CityName
        }).ToList();

        viewModel.Filter = filter;
        viewModel.Filter.LocationOptions = filterOptions.Locations.Select(fo => new FilterOptionViewModel
        {
            Value = fo.Value,
            Label = fo.Label,
            Count = fo.Count
        }).ToList();
        // ... repeat for PropertyTypeOptions

        viewModel.Pagination = new PaginationViewModel
        {
            CurrentPage = page,
            PageSize = 12,
            TotalItems = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / 12m)
        };

        ViewBag.ActivePage = "Properties";
        return View(viewModel);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading properties");
        return RedirectToAction("Error", "Home");
    }
}
```

##### 4. **View** (Display ViewModel)
**File:** `Views/Properties/Index.cshtml`

```html
@model PropertyListViewModel

<!-- Filter Form -->
<form method="get" asp-action="Index">
    <input asp-for="Filter.Keyword" placeholder="Search..." />
    <select asp-for="Filter.Location">
        <option value="">All Locations</option>
        @foreach (var option in Model.Filter.LocationOptions)
        {
            <option value="@option.Value">@option.Label (@option.Count)</option>
        }
    </select>
    <button type="submit">Search</button>
</form>

<!-- Property Grid -->
<div class="properties-grid">
    @foreach (var property in Model.Properties)
    {
        <div class="property-card">
            <img src="@property.ThumbnailUrl" alt="@property.Title" />
            <h3>@property.Title</h3>
            <p>$@property.Price.ToString("N0")</p>
            <p>@property.Bedrooms Beds | @property.Bathrooms Baths | @property.AreaSqft sqft</p>
            <a href="/Properties/Details/@property.Id">View Details</a>
        </div>
    }
</div>

<!-- Pagination -->
@if (Model.Pagination.TotalPages > 1)
{
    <div class="pagination">
        @if (Model.Pagination.HasPreviousPage)
        {
            <a href="@Url.Action("Index", new { page = Model.Pagination.PreviousPage })">Previous</a>
        }

        @for (int i = 1; i <= Model.Pagination.TotalPages; i++)
        {
            <a href="@Url.Action("Index", new { page = i })" 
               class="@(i == Model.Pagination.CurrentPage ? "active" : "")">
                @i
            </a>
        }

        @if (Model.Pagination.HasNextPage)
        {
            <a href="@Url.Action("Index", new { page = Model.Pagination.NextPage })">Next</a>
        }
    </div>
}
```

---

### Dependency Injection Setup

**File:** `Program.cs`

```csharp
// Add these lines in Program.cs ConfigureServices

// Repositories
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();

// Services
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IContactService, ContactService>();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
```

**File:** `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=RealtorsPortal;User Id=sa;Password=YOUR_PASSWORD;"
  }
}
```

---

### Common Mapping Pattern

```csharp
// Entity → DTO (in Service layer)
var propertyDto = new PropertyResultDto
{
    Id = entity.Id,
    Slug = entity.Slug,
    Title = entity.Title,
    // ... copy all properties
};

// DTO → ViewModel (in Controller layer)
var propertyVm = new PropertyCardViewModel
{
    Id = dto.Id,
    Slug = dto.Slug,
    Title = dto.Title,
    // ... copy all properties
};
```

**Tip:** Consider using AutoMapper later for large projects:
```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Property, PropertyResultDto>();
    cfg.CreateMap<PropertyResultDto, PropertyCardViewModel>();
});
```

---

### Testing Your Implementation

```csharp
// In Program.cs, add console logging for debugging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Debug);
});

// In repository/service, log queries
_logger.LogInformation("Querying properties with filter: {@Filter}", filter);
var results = await query.ToListAsync();
_logger.LogInformation("Found {Count} properties", results.Count);
```

---

### Checklist for Each Feature

- [ ] Create entity relationships (already done in ApplicationDbContext)
- [ ] Implement repository method (use EF Core queries)
- [ ] Implement service method (entity → DTO mapping)
- [ ] Update/create controller action (DTO → ViewModel mapping)
- [ ] Update view to use ViewModel
- [ ] Add error handling and logging
- [ ] Test with SQL Server database
- [ ] Verify pagination works
- [ ] Test all filter combinations

---

### Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| N+1 queries | Use `.Include()` in repository to load related entities |
| Null reference | Check if relationship is null before accessing (e.g., `p.Agent?.FullName`) |
| Circular reference | Use DTO to break circular reference (Entity not returned from Service) |
| Slow queries | Add SQL indexes on foreign keys and filter columns |
| Mapping errors | Ensure all properties match between Entity, DTO, ViewModel |
| View not updating | Clear browser cache, rebuild solution |

---

### File Dependencies (Read in This Order)

1. **Models/Entities/** - Understand the database structure
2. **Models/DTOs/ServiceDtos.cs** - Understand data contracts
3. **Models/ViewModels/** - Understand UI requirements
4. **Repositories/Interfaces/** - Understand data access contracts
5. **Services/Interfaces/** - Understand business logic contracts
6. **Controllers/** - See how it all connects
7. **Documentation/ArchitectureOverview.cs** - Deep dive into architecture

---

**Ready to start? Begin with:**
1. `PropertyRepository.GetFeaturedAsync()` - Easiest starting point
2. `PropertyService.GetFeaturedPropertiesAsync()` - Simple DTO mapping
3. `HomeController.Index()` - Minimal UI required

Good luck! 🚀
