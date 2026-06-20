using System;

namespace RealtorsPortal.Models.Entities
{
    public class PropertyImage
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }

        public string Url { get; set; } = null!;
        public string? Caption { get; set; }
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }

        // Navigation
        public Property? Property { get; set; }
    }
}
