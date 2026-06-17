using System.Collections.Generic;

namespace RealtorsPortal.Models.Entities
{
    public class Area
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CityId { get; set; }

        // Navigation
        public City? City { get; set; }
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
