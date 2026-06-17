using System.Collections.Generic;

namespace RealtorsPortal.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Slug { get; set; }
        public string? Description { get; set; }

        // Navigation
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
