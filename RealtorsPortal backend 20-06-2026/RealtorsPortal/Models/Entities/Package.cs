namespace RealtorsPortal.Models.Entities
{
    public class Package
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;           // "Basic" | "Gold" | "Platinum"
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public int? ListingLimit { get; set; }               // null = unlimited
        public int? ImageLimit { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<SubscriptionHistory> SubscriptionHistories { get; set; } = new List<SubscriptionHistory>();
    }
}
