using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Reviews
    {
        public Guid Id { get; set; } = new Guid();
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Rating { get; set; }
        [ForeignKey(nameof(Product))]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        [ForeignKey(nameof(User))]
        public Guid userId { get; set; }
        public User user { get; set; }
    }
}
