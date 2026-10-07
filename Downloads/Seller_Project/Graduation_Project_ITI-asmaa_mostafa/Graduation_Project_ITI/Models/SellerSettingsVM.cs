using System.ComponentModel.DataAnnotations;

namespace Graduation_Project_ITI.Models
{
    public class SellerSettingsVM
    {
        // =========================
        // Account Information
        // =========================

        public Guid SellerId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }


        // =========================
        // Store Information
        // =========================

        [Required]
        [StringLength(80, MinimumLength = 5)]
        public string ShopName { get; set; }

        public bool StoreStatus { get; set; }

        public int? IsApproved { get; set; }

        public DateTime CreatedAt { get; set; }


        // =========================
        // Security
        // =========================

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string? ConfirmPassword { get; set; }
    }
}