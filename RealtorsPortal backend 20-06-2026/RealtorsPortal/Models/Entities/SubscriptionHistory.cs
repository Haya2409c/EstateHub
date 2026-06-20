namespace RealtorsPortal.Models.Entities
{
    public class SubscriptionHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int PackageId { get; set; }
        public int? PaymentId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Active";     // "Active" | "Expired" | "Cancelled"
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Package Package { get; set; } = null!;
        public Payment? Payment { get; set; }
    }
}
