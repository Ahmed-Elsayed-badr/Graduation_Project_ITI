using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Order
    {
        public Guid Id { get; set; } = new Guid();
        public DateTime OrderDate { get; set; } = DateTime.Now;
        [Required(ErrorMessage ="Status Is Required")]
        public string Status { get; set; }
        [Range(0,100000000)]
        public decimal TotalAmount { get; set; }
        [ForeignKey(nameof(User))]
        public Guid userId { get; set; }
        public User user { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
