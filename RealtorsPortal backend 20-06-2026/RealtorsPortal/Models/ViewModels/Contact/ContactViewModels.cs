using System.ComponentModel.DataAnnotations;

namespace RealtorsPortal.Models.ViewModels.Contact
{
    /// <summary>
    /// ViewModel for Contact/Index (contact page).
    /// Displays: contact form, office locations, map
    /// Data Sources:
    ///   - IContactService.GetOfficeLocationsAsync()
    /// Controller Actions: 
    ///   - ContactController.Index() [GET]
    ///   - ContactController.Index(ContactFormViewModel) [POST]
    /// </summary>
    public class ContactPageViewModel
    {
        public ContactFormViewModel Form { get; set; } = new();
        public List<OfficeLocationViewModel> Offices { get; set; } = new();
    }

    /// <summary>
    /// Contact form (general enquiry).
    /// Submitted from: Contact Us page
    /// Target Service Action: IContactService.SubmitGeneralEnquiryAsync()
    /// Also used for: Property enquiry, Agent enquiry (with additional context)
    /// </summary>
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = null!;

        [Phone]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(200)]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters.")]
        public string Message { get; set; } = null!;
    }

    /// <summary>
    /// Office location information.
    /// Displayed on Contact page with map, phone, email, hours (if applicable)
    /// </summary>
    public class OfficeLocationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Hours { get; set; }
    }
}
