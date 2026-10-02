using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(50, ErrorMessage = "Name must be 50 characters or fewer")]
        [MinLength(5, ErrorMessage = "Name must be at least 5 characters")]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [MaxLength(500, ErrorMessage = "Description must be 500 characters or fewer")]
        [MinLength(20, ErrorMessage = "Description must be at least 20 characters")]
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        [Range(0, 1000000000, ErrorMessage = "Price must be between 0 and 1,000,000,000")]
        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        [Required(ErrorMessage = "Product image is required")]
        public string ImageUrl  { get; set; }
       
       
        public ICollection<OrderItem> OrderItems { get; set; }
        [ForeignKey(nameof(Category))]
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        [ForeignKey(nameof(Seller))]
        public Guid SellerId { get; set; }
        public Seller Seller { get; set; }
        public ICollection<CartIItem> CartItems { get; set; } = new List<CartIItem>();
        public ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();

    }
}
