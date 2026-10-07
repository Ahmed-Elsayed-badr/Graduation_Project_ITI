using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Project_ITI.ViewModels
{
    public class ProductViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Product Name")]
        [Required(ErrorMessage = "Product name is required")]
        [MinLength(5, ErrorMessage = "Name must be at least 5 characters")]
        [MaxLength(50, ErrorMessage = "Name must be 50 characters or fewer")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [Required(ErrorMessage = "Description is required")]
        [MinLength(20, ErrorMessage = "Description must be at least 20 characters")]
        [MaxLength(500, ErrorMessage = "Description must be 500 characters or fewer")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Price")]
        [Required(ErrorMessage = "Price is required")]
        [Range(0, 1000000000, ErrorMessage = "Price must be between 0 and 1,000,000,000")]
        public decimal Price { get; set; }

        [Display(Name = "Available Quantity")]
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int AvailableQuantity { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Category")]
        [Required(ErrorMessage = "Please select a category")]
        public Guid CategoryId { get; set; }

        // Set by the server (not entered by the user). See TODO(Auth) in ProductService.
        public Guid SellerId { get; set; }

        public List<SelectListItem>? Categories { get; set; }
    }
}
