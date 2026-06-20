using System;

namespace RealtorsPortal.Models.Entities
{
    public class PropertyEnquiry
    {
        public int Id { get; set; }
        public int? PropertyId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Message { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
        public string? OwnerUserId { get; set; } // Seller/Agent userId who owns the property

        public Property? Property { get; set; }
    }
}
