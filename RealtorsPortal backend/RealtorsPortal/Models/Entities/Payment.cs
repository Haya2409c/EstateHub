namespace RealtorsPortal.Models.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = null!;  // PayPal order/capture ID
        public string UserId { get; set; } = null!;
        public int PackageId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Pending";     // "Pending" | "Completed" | "Failed" | "Refunded"
        public string? GatewayResponse { get; set; }        // raw JSON from payment gateway
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Package Package { get; set; } = null!;
        public SubscriptionHistory? SubscriptionHistory { get; set; }
    }
}
