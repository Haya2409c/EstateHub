using System.Collections.Generic;
using System;

namespace RealtorsPortal.Models.Entities
{
    public class Agent
    {
        // Using int as PK for simplicity; could be string/slug or GUID depending on auth strategy
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? Slug { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Bio { get; set; }
        public int? YearsExperience { get; set; }
        public double? Rating { get; set; }

        // Navigation
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
