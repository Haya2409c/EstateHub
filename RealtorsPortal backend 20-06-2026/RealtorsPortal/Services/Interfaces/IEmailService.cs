namespace RealtorsPortal.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);

        Task SendRegistrationConfirmationAsync(
            string toEmail,
            string fullName,
            string accountType);

        Task SendPackageExpiryNotificationAsync(
            string toEmail,
            string fullName,
            string packageName,
            DateTime startDate,
            DateTime expiryDate,
            int daysLeft,
            string? listingLimit);
    }
}
