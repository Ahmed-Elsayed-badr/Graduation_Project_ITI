using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace DAL
{
    public class User
    {
        public Guid Id { get; set; } = new Guid();
        [MaxLength(50,ErrorMessage ="Name must be less than or equal 50 character")]
        [MinLength(3,ErrorMessage = "Name must be greater than or equal 3 character")]
        [Required(ErrorMessage ="Name Is Required")]
        public string Name { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email Is Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password Is Required")]
        public string PasswordHash { get; set; }
        [Phone]
        public string? phonenumber { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public int IsActive {  get; set; }
        [Required(ErrorMessage ="Role Is Required")]
        public string Role { get; set; }

        public ICollection<Order> order { get; set; } = new List<Order>();
        public ICollection<Role> role   { get; set; }
        public Cart cart { get; set; }

        [ForeignKey(nameof(WishList))]
        public Guid WishListId { get; set; }
        public WishList WishList { get; set; }
        public ICollection<Reviews> reviews { get; set; } = new List<Reviews>();
        public Seller? Seller { get; set; }
    }
}
