using System.ComponentModel.DataAnnotations;

namespace RealtorsPortal.Models.ViewModels
{
    public class ProfileUpdateViewModel
    {
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public IFormFile? Photo { get; set; }

        public string? CurrentPassword { get; set; }

        [MinLength(8, ErrorMessage = "New password must be at least 8 characters.")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
