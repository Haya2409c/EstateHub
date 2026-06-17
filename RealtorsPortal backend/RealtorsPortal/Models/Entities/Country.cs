using System.Collections.Generic;

namespace RealtorsPortal.Models.Entities
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? IsoCode { get; set; }

        // Navigation
        public ICollection<Region> Regions { get; set; } = new List<Region>();
    }
}
