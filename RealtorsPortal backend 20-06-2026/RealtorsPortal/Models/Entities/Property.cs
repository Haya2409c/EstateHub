using System;
using System.Collections.Generic;

namespace RealtorsPortal.Models.Entities
{
    public class Property
    {
        public int Id { get; set; }

        // SEO / routing
        public string? Slug { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        // Pricing & specs
        public decimal Price { get; set; }
        public string? PropertyType { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public decimal? AreaSqft { get; set; }

        // Listing type: buy / rent / sell
        public string? Status { get; set; }

        // Admin approval state: Pending / Active / Rejected
        public string ApprovalStatus { get; set; } = "Pending";

        // Comma-separated amenity tags e.g. "wifi,gym,pool"
        public string? Amenities { get; set; }

        // Flags
        public bool IsFeatured { get; set; }

        // Location
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTime ListedDate { get; set; }

        // Foreign keys
        public int CategoryId { get; set; }
        public int? AgentId { get; set; }
        public int? AreaId { get; set; }
        public string? SellerId { get; set; } // FK to AspNetUsers; null if agent-managed

        // Navigation properties
        public Category? Category { get; set; }
        public Agent? Agent { get; set; }
        public Area? Area { get; set; }

        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    }
}
