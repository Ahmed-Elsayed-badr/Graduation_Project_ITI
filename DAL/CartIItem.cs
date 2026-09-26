using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class CartIItem
    {
        public Guid Id { get; set; } = new Guid();
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        [ForeignKey(nameof(Cart))]
        public Guid CartId { get; set; }
        public Cart cart { get; set; }
        [ForeignKey(nameof(Product))]
        public Guid productId { get; set; }
        public Product product { get; set; }
    }
}
