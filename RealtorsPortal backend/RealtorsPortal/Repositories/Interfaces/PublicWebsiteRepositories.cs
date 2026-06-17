using RealtorsPortal.Models.Entities;

namespace RealtorsPortal.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository interface for common CRUD operations.
    /// TEntity: entity type, TKey: primary key type
    /// All specific repositories inherit from this.
    /// </summary>
    public interface IGenericRepository<TEntity, TKey> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TKey id);
    }

    /// <summary>
    /// Property Repository Interface.
    /// Specific queries for property data access.
    /// Data Flow: Database -> Entity -> DTO (in Service)
    /// </summary>
    public interface IPropertyRepository : IGenericRepository<Property, int>
    {
        /// <summary>
        /// Get featured properties (limited).
        /// </summary>
        Task<List<Property>> GetFeaturedAsync(int count);

        /// <summary>
        /// Search/filter properties with pagination.
        /// Includes: Category, Agent, Area, primary PropertyImage
        /// </summary>
        Task<(List<Property> properties, int totalCount)> SearchAsync(
            string? keyword,
            string? location,
            string? propertyType,
            decimal? minPrice,
            decimal? maxPrice,
            int? minBedrooms,
            int? minBathrooms,
            decimal? minArea,
            string? sortBy,
            int page,
            int pageSize);

        /// <summary>
        /// Get single property with all related data.
        /// Includes: Images, Category, Agent, Area
        /// </summary>
        Task<Property?> GetWithDetailsAsync(int id);

        /// <summary>
        /// Get similar properties (same category, similar price).
        /// </summary>
        Task<List<Property>> GetSimilarAsync(int propertyId, int count);

        /// <summary>
        /// Get distinct locations (Areas) with property count.
        /// </summary>
        Task<List<(int id, string name, int count)>> GetDistinctLocationsAsync();

        /// <summary>
        /// Get distinct property types with count.
        /// </summary>
        Task<List<(string type, int count)>> GetDistinctTypesAsync();

        /// <summary>
        /// Get properties by agent (with pagination).
        /// </summary>
        Task<(List<Property> properties, int totalCount)> GetByAgentAsync(
            int agentId,
            int page,
            int pageSize);

        /// <summary>
        /// Get total property count.
        /// </summary>
        Task<int> GetTotalCountAsync();
    }

    /// <summary>
    /// Agent Repository Interface.
    /// Specific queries for agent data access.
    /// Data Flow: Database -> Entity -> DTO (in Service)
    /// </summary>
    public interface IAgentRepository : IGenericRepository<Agent, int>
    {
        /// <summary>
        /// Get agent by slug (e.g., "hifza", "tayyaba").
        /// </summary>
        Task<Agent?> GetBySlugAsync(string slug);

        /// <summary>
        /// Get top agents ordered by rating/experience.
        /// </summary>
        Task<List<Agent>> GetTopAgentsAsync(int count);

        /// <summary>
        /// Search/filter agents with pagination.
        /// </summary>
        Task<(List<Agent> agents, int totalCount)> SearchAsync(
            string? specialization,
            string? location,
            string? sortBy,
            int page,
            int pageSize);

        /// <summary>
        /// Get total agent count.
        /// </summary>
        Task<int> GetTotalCountAsync();
    }

    /// <summary>
    /// News Repository Interface.
    /// Specific queries for news data access.
    /// Data Flow: Database -> Entity -> DTO (in Service)
    /// </summary>
    public interface INewsRepository : IGenericRepository<News, int>
    {
        /// <summary>
        /// Get paginated articles ordered by PublishedAt DESC.
        /// </summary>
        Task<(List<News> articles, int totalCount)> GetPagedAsync(int page, int pageSize);

        /// <summary>
        /// Get recent articles for "related articles" section.
        /// </summary>
        Task<List<News>> GetRecentAsync(int count, int excludeId);
    }

    /// <summary>
    /// Contact Message Repository Interface.
    /// Stores contact form submissions.
    /// Data Flow: Service -> Entity -> Database
    /// </summary>
    public interface IContactRepository
    {
        /// <summary>
        /// Save contact message to database.
        /// </summary>
        Task SaveMessageAsync(ContactMessage message);

        /// <summary>
        /// Get office locations (hardcoded or from table).
        /// </summary>
        Task<List<OfficeLocation>> GetOfficeLocationsAsync();
    }

    /// <summary>
    /// Placeholder: Office Location entity (future implementation).
    /// For now, office locations can be hardcoded in repository or stored in a simple table.
    /// </summary>
    public class OfficeLocation
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
}
