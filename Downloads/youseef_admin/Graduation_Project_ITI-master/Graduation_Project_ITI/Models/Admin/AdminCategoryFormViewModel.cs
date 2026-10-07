using System.ComponentModel.DataAnnotations;

namespace Graduation_Project_ITI.Models.Admin
{
    public class AdminCategoryFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name Is Required")]
        [MaxLength(100, ErrorMessage = "Name must be 100 characters or less")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300, ErrorMessage = "Description must be 300 characters or less")]
        [Display(Name = "Description")]
        public string? Comment { get; set; }
    }
}