//using Microsoft.AspNetCore.Antiforgery;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Seller
    {
        public Guid Id { get; set; } = new Guid();
        [ForeignKey(nameof(User))]
        public Guid? userId { get; set; }
        public User? user { get; set; }
        [Required(ErrorMessage ="ShopeName IS Required")]
        [MaxLength(80,ErrorMessage ="Shope name must be less than or equal 80 char")]
        [MinLength(5,ErrorMessage ="Shop Name must be grater than or equal 5 char ")]
        public string ShopName { get; set; }
        
        public int? IsApproved { get; set; }
        [Required(ErrorMessage ="Status Is Required")]
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<Product> Products { get; set; }

    }
}
