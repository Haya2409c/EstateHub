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
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string Subject { get; set; } = null!;
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
