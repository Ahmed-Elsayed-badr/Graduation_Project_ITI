using System.ComponentModel.DataAnnotations;

namespace DAL
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        // Optional free-text description. Stored as empty string (never null) because the DB column is NOT NULL.
        [Display(Name = "Description")]
        public string Comment { get; set; } = string.Empty;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
