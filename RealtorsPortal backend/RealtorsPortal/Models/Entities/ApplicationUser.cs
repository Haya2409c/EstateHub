using Microsoft.AspNetCore.Identity;

namespace RealtorsPortal.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public string? PhotoUrl { get; set; }
        public string AccountType { get; set; } = "Seller"; // "Seller" | "Agent" | "Admin"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public SellerProfile? SellerProfile { get; set; }
    }
}
