using Microsoft.AspNetCore.Mvc;
using RealtorsPortal.Models.ViewModels.Contact;
using RealtorsPortal.Services.Interfaces;

namespace RealtorsPortal.Controllers
{
    /// <summary>
    /// Contact Controller
    /// Handles contact form submission and office information display.
    /// 
    /// Data Flow:
    /// 1. GET: Loads office locations from database
    /// 2. POST: Form submission -> IContactService -> Repository -> Database
    /// 
    /// Dependencies:
    ///   - IContactService: form submission, office locations
    ///   - IEmailService: send notification emails (called by IContactService)
    /// </summary>
    public class ContactController : Controller
    {
        // TODO: Inject IContactService, ILogger

        public ContactController()
        {
            // Constructor placeholder
        }

        /// <summary>
        /// GET: /Contact or /Contact/Index
        /// Contact page with form and office locations.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new ContactPageViewModel();

                // TODO: Implement
                // Steps:
                //   1. Call IContactService.GetOfficeLocationsAsync()
                //   2. Map OfficeLocationDto list to OfficeLocationViewModel list
                //   3. Assign to viewModel.Offices
                //   4. Initialize viewModel.Form (empty)
                //   5. Return View(viewModel)

                ViewBag.ActivePage = "Contact";
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// POST: /Contact or /Contact/Index
        /// Submit contact form.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload office locations if form validation fails
                    var viewModel = new ContactPageViewModel
                    {
                        Form = model,
                        Offices = new List<OfficeLocationViewModel>()
                        // TODO: Reload offices via IContactService
                    };
                    return View(viewModel);
                }

                // TODO: Implement
                // Steps:
                //   1. Create ContactMessageDto from model
                //   2. Call IContactService.SubmitGeneralEnquiryAsync(dto)
                //   3. Show success message (TempData, ViewBag, or redirect to success page)
                //   4. Redirect to success page or return View with success message

                TempData["SuccessMessage"] = "Thank you! Your message has been sent successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                ModelState.AddModelError("", "Failed to submit form. Please try again.");
                ViewBag.ActivePage = "Contact";
                return View(model);
            }
        }
    }
}
