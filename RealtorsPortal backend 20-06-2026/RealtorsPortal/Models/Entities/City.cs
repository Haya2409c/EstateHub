using System.Collections.Generic;

namespace RealtorsPortal.Models.Entities
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int RegionId { get; set; }

        // Navigation
        public Region? Region { get; set; }
        public ICollection<Area> Areas { get; set; } = new List<Area>();
    }
}
