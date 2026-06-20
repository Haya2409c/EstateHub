using Microsoft.AspNetCore.Mvc;
using RealtorsPortal.Data;
using RealtorsPortal.Models.Entities;
using RealtorsPortal.Models.ViewModels.Contact;
using RealtorsPortal.Services.Interfaces;

namespace RealtorsPortal.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactController> _logger;
        private readonly string _adminEmail;

        public ContactController(
            ApplicationDbContext db,
            IEmailService emailService,
            ILogger<ContactController> logger,
            IConfiguration config)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
            _adminEmail = config["Email:FromAddress"] ?? "admin@realtorsportal.com";
        }

        // GET: /Contact
        public IActionResult Index()
        {
            ViewBag.ActivePage = "Contact";
            return View(new ContactPageViewModel());
        }

        // POST: /Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactPageViewModel pageModel)
        {
            ViewBag.ActivePage = "Contact";

            if (!ModelState.IsValid)
                return View(pageModel);

            var model = pageModel.Form;

            _db.ContactMessages.Add(new ContactMessage
            {
                FullName    = model.FullName,
                Email       = model.Email,
                Phone       = model.Phone,
                Subject     = model.Subject,
                Message     = model.Message,
                SubmittedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        _adminEmail,
                        "Admin",
                        $"New Contact Message: {model.Subject}",
                        $"<h3>New message from {model.FullName}</h3>" +
                        $"<p><b>Email:</b> {model.Email}</p>" +
                        $"<p><b>Phone:</b> {model.Phone ?? "—"}</p>" +
                        $"<p><b>Subject:</b> {model.Subject}</p>" +
                        $"<p><b>Message:</b><br>{model.Message}</p>"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Admin notification email failed for contact from {Email}", model.Email);
                }
            });

            TempData["Success"] = "Thank you! Your message has been sent. We'll get back to you shortly.";
            return RedirectToAction(nameof(Index));
        }
    }
}
