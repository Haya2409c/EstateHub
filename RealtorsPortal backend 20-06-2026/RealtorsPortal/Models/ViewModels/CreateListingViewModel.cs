using System.ComponentModel.DataAnnotations;

namespace RealtorsPortal.Models.ViewModels
{
    public class CreateListingViewModel
    {
        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string PropertyType { get; set; } = string.Empty;

        // buy / rent / sell
        [Required]
        public string Status { get; set; } = "buy";

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public decimal? AreaSqft { get; set; }

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

        // comma-separated amenity values e.g. "wifi,gym,pool"
        public List<string> Amenities { get; set; } = new();

        public List<IFormFile> Images { get; set; } = new();
    }
}
