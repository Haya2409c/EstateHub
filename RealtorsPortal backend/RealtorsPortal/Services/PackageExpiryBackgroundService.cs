using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealtorsPortal.Data;
using RealtorsPortal.Models.Entities;
using RealtorsPortal.Services.Interfaces;

namespace RealtorsPortal.Services
{
    public class PackageExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PackageExpiryBackgroundService> _logger;

        // Fire once per day; first run after a short startup delay
        private static readonly TimeSpan CheckInterval  = TimeSpan.FromHours(24);
        private static readonly TimeSpan StartupDelay   = TimeSpan.FromSeconds(30);

        public PackageExpiryBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<PackageExpiryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PackageExpiryBackgroundService starting.");

            // Small startup delay so the app fully initialises before first run
            await Task.Delay(StartupDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndNotifyAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error during expiry check cycle.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }

            _logger.LogInformation("PackageExpiryBackgroundService stopping.");
        }

        private async Task CheckAndNotifyAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db           = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var userManager  = scope.ServiceProvider
                                    .GetRequiredService<UserManager<ApplicationUser>>();

            var now       = DateTime.UtcNow.Date;
            var threshold = now.AddDays(7);

            // Subscriptions that are Active AND expire within the next 7 days
            var expiring = await db.SubscriptionHistories
                .Where(s => s.Status == "Active"
                         && s.EndDate.Date >= now
                         && s.EndDate.Date <= threshold)
                .Include(s => s.Package)
                .ToListAsync(ct);

            if (!expiring.Any())
            {
                _logger.LogInformation("No expiring subscriptions found on {Date}.", now);
                return;
            }

            _logger.LogInformation("{Count} expiring subscription(s) found.", expiring.Count);

            foreach (var sub in expiring)
            {
                ct.ThrowIfCancellationRequested();

                var user = await userManager.FindByIdAsync(sub.UserId);
                if (user is null || string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogWarning("Subscription {SubId}: user {UserId} not found, skipping.",
                        sub.Id, sub.UserId);
                    continue;
                }

                var daysLeft = (sub.EndDate.Date - now).Days;

                // Already notified today? (check EmailLog to avoid spam)
                bool alreadySent = await db.EmailLogs.AnyAsync(
                    e => e.ToEmail     == user.Email
                      && e.EmailType   == "SubscriptionExpiry"
                      && e.Status      == "Sent"
                      && e.SentAt.HasValue
                      && e.SentAt.Value.Date == now,
                    ct);

                if (alreadySent)
                {
                    _logger.LogDebug("Expiry email already sent to {Email} today.", user.Email);
                    continue;
                }

                string? listingLimit = sub.Package?.ListingLimit?.ToString();

                await emailService.SendPackageExpiryNotificationAsync(
                    toEmail:     user.Email,
                    fullName:    user.FullName ?? user.UserName ?? user.Email,
                    packageName: sub.Package?.Name ?? "Your Plan",
                    startDate:   sub.StartDate,
                    expiryDate:  sub.EndDate,
                    daysLeft:    daysLeft,
                    listingLimit: listingLimit);

                _logger.LogInformation("Expiry notification sent to {Email} ({DaysLeft}d left).",
                    user.Email, daysLeft);
            }
        }
    }
}
