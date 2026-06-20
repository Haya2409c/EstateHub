using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using RealtorsPortal.Data;
using RealtorsPortal.Models.Entities;
using RealtorsPortal.Services.Interfaces;

namespace RealtorsPortal.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailService> _logger;

        private readonly string _fromName;
        private readonly string _fromAddress;
        private readonly string _siteUrl;
        private readonly string _supportEmail;
        private readonly string _provider;
        private readonly string _templateDir;

        public EmailService(
            IConfiguration config,
            IServiceScopeFactory scopeFactory,
            ILogger<EmailService> logger,
            IWebHostEnvironment env)
        {
            _config        = config;
            _scopeFactory  = scopeFactory;
            _logger        = logger;
            _fromName      = config["Email:FromName"]    ?? "RealtorsPortal";
            _fromAddress   = config["Email:FromAddress"] ?? "noreply@realtorsportal.com";
            _siteUrl       = config["Email:SiteUrl"]     ?? "http://localhost:50873";
            _supportEmail  = config["Email:SupportEmail"] ?? _fromAddress;
            _provider      = config["Email:Provider"]    ?? "Smtp";
            _templateDir   = Path.Combine(env.ContentRootPath, "Services", "EmailTemplates");
        }

        // ── Generic send ──────────────────────────────────────
        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var log = new EmailLog
            {
                ToEmail   = toEmail,
                Subject   = subject,
                Body      = htmlBody,
                EmailType = "Generic",
                Status    = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                if (_provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
                    await SendViaSendGridAsync(toEmail, toName, subject, htmlBody);
                else
                    await SendViaSmtpAsync(toEmail, toName, subject, htmlBody);

                log.Status = "Sent";
                log.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                log.Status       = "Failed";
                log.ErrorMessage = ex.Message.Length > 950 ? ex.Message[..950] : ex.Message;
                log.RetryCount++;
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
            finally
            {
                await PersistLogAsync(log);
            }
        }

        // ── Registration confirmation ─────────────────────────
        public async Task SendRegistrationConfirmationAsync(
            string toEmail, string fullName, string accountType)
        {
            var html = await LoadTemplateAsync("RegistrationConfirmation.html");
            html = html
                .Replace("{{FullName}}",     fullName)
                .Replace("{{Email}}",        toEmail)
                .Replace("{{AccountType}}", accountType)
                .Replace("{{RegisteredAt}}", DateTime.UtcNow.ToString("dd MMM yyyy, HH:mm UTC"))
                .Replace("{{DashboardUrl}}", $"{_siteUrl}/Account/Login")
                .Replace("{{SupportEmail}}", _supportEmail)
                .Replace("{{SiteUrl}}",      _siteUrl)
                .Replace("{{Year}}",         DateTime.UtcNow.Year.ToString());

            await SendWithLogTypeAsync(toEmail, fullName,
                subject: $"Welcome to RealtorsPortal — Registration Confirmed",
                htmlBody: html,
                emailType: "WelcomeEmail");
        }

        // ── Package expiry notification ───────────────────────
        public async Task SendPackageExpiryNotificationAsync(
            string toEmail, string fullName, string packageName,
            DateTime startDate, DateTime expiryDate, int daysLeft,
            string? listingLimit)
        {
            var html = await LoadTemplateAsync("PackageExpiryNotification.html");
            html = html
                .Replace("{{FullName}}",      fullName)
                .Replace("{{PackageName}}",   packageName)
                .Replace("{{DaysLeft}}",      daysLeft.ToString())
                .Replace("{{StartDate}}",     startDate.ToString("dd MMM yyyy"))
                .Replace("{{ExpiryDate}}",    expiryDate.ToString("dd MMM yyyy"))
                .Replace("{{ListingLimit}}",  listingLimit ?? "Unlimited")
                .Replace("{{RenewUrl}}",      $"{_siteUrl}/Seller/Packages")
                .Replace("{{PackagesUrl}}",   $"{_siteUrl}/Seller/Packages")
                .Replace("{{SupportEmail}}",  _supportEmail)
                .Replace("{{SiteUrl}}",       _siteUrl)
                .Replace("{{Year}}",          DateTime.UtcNow.Year.ToString());

            await SendWithLogTypeAsync(toEmail, fullName,
                subject: $"Your {packageName} plan expires in {daysLeft} day(s) — Renew now",
                htmlBody: html,
                emailType: "SubscriptionExpiry");
        }

        // ── Private helpers ───────────────────────────────────
        private async Task SendWithLogTypeAsync(
            string toEmail, string toName, string subject, string htmlBody, string emailType)
        {
            var log = new EmailLog
            {
                ToEmail   = toEmail,
                Subject   = subject,
                Body      = htmlBody,
                EmailType = emailType,
                Status    = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                if (_provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
                    await SendViaSendGridAsync(toEmail, toName, subject, htmlBody);
                else
                    await SendViaSmtpAsync(toEmail, toName, subject, htmlBody);

                log.Status = "Sent";
                log.SentAt = DateTime.UtcNow;
                _logger.LogInformation("Email [{Type}] sent to {Email}", emailType, toEmail);
            }
            catch (Exception ex)
            {
                log.Status       = "Failed";
                log.ErrorMessage = ex.Message.Length > 950 ? ex.Message[..950] : ex.Message;
                log.RetryCount++;
                _logger.LogError(ex, "Failed to send [{Type}] email to {Email}", emailType, toEmail);
                // Don't rethrow — background service should not crash on email failure
            }
            finally
            {
                await PersistLogAsync(log);
            }
        }

        private async Task SendViaSmtpAsync(
            string toEmail, string toName, string subject, string htmlBody)
        {
            var smtpCfg = _config.GetSection("Email:Smtp");
            var host    = smtpCfg["Host"]     ?? "smtp.gmail.com";
            var port    = int.Parse(smtpCfg["Port"] ?? "587");
            var user    = smtpCfg["Username"] ?? "";
            var pass    = smtpCfg["Password"] ?? "";
            var ssl     = bool.Parse(smtpCfg["EnableSsl"] ?? "true");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromAddress));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body    = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port,
                ssl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            if (!string.IsNullOrWhiteSpace(user))
                await client.AuthenticateAsync(user, pass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private async Task SendViaSendGridAsync(
            string toEmail, string toName, string subject, string htmlBody)
        {
            var apiKey = _config["Email:SendGrid:ApiKey"] ?? "";
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var body = new
            {
                personalizations = new[] { new { to = new[] { new { email = toEmail, name = toName } } } },
                from    = new { email = _fromAddress, name = _fromName },
                subject,
                content = new[] { new { type = "text/html", value = htmlBody } }
            };

            var json    = System.Text.Json.JsonSerializer.Serialize(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var res     = await http.PostAsync("https://api.sendgrid.com/v3/mail/send", content);

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"SendGrid error {res.StatusCode}: {err}");
            }
        }

        private async Task<string> LoadTemplateAsync(string fileName)
        {
            var path = Path.Combine(_templateDir, fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Email template not found: {path}");
            return await File.ReadAllTextAsync(path);
        }

        private async Task PersistLogAsync(EmailLog log)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.EmailLogs.Add(log);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist EmailLog for {Email}", log.ToEmail);
            }
        }
    }
}
