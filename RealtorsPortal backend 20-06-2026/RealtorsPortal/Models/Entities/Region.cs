using System.Collections.Generic;

namespace RealtorsPortal.Models.Entities
{
    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CountryId { get; set; }

        // Navigation
        public Country? Country { get; set; }
        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
