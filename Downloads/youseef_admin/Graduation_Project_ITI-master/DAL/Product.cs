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
        public Guid Id { get; set; } = new Guid();
        [MaxLength(50, ErrorMessage = "Name must be less than or equal 50 character")]
        [MinLength(5, ErrorMessage = "Name must be greater than or equal 3 character")]
        [Required(ErrorMessage = "Name Is Required")]
        public string Name { get; set; }
        [MaxLength(500, ErrorMessage = "Description must be less than or equal 50 character")]
        [MinLength(20, ErrorMessage = "Description must be greater than or equal 3 character")]
        [Required(ErrorMessage = "Description Is Required")]
        public string Description { get; set; }
        [Range(0,1000000000,ErrorMessage ="price must be range from 0 To 1000000000")]
        [Required(ErrorMessage = "Price Is Required")]
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        [Required(ErrorMessage = "ImageUrl Is Required")]
        public string ImageUrl  { get; set; }
        [ForeignKey(nameof(Role))]
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
        public ICollection<OrderItem> orderItems { get; set; }
        [ForeignKey(nameof(Category))]
        public Guid categoryId { get; set; }
        public Category category { get; set; }
        [ForeignKey(nameof(Seller))]
        public Guid sellerId { get; set; }
        public Seller Seller { get; set; }
        public ICollection<CartIItem> cartItems {  get; set; } = new List<CartIItem>();
        public ICollection<Reviews> reviews { get; set; } = new List<Reviews>();

    }
}
