namespace RealtorsPortal.Models.Entities
{
    public class SellerProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string? CompanyName { get; set; }
        public string PackageTier { get; set; } = "Basic"; // "Basic" | "Premium" | "Elite"
        public DateTime? PackageExpiry { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
    }
}
