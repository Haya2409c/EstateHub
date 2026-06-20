namespace RealtorsPortal.Models.Entities
{
    public class EmailLog
    {
        public int Id { get; set; }
        public string? RecipientUserId { get; set; }
        public string ToEmail { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string? Body { get; set; }
        public string EmailType { get; set; } = null!;      // "WelcomeEmail" | "PaymentConfirmation" | "SubscriptionExpiry" | "PasswordReset"
        public string Status { get; set; } = "Pending";    // "Pending" | "Sent" | "Failed"
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ApplicationUser? RecipientUser { get; set; }
    }
}
