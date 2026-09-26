using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Cart
    {
        public Guid Id { get; set; } = new Guid();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey(nameof(User))]
        public Guid userId { get; set; }
        public User user { get; set; }
        public ICollection<CartIItem> cartIItems { get; set; } = new List<CartIItem>(); 
    }
}
