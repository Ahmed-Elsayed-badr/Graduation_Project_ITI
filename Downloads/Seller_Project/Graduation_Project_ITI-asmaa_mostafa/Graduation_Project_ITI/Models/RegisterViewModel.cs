using System.ComponentModel.DataAnnotations;

namespace Graduation_Project_ITI.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name Is Required")]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email Is Required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password Is Required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Role Is Required")]
        public string Role { get; set; } = string.Empty;

        public string? ShopName { get; set; }
    }
}