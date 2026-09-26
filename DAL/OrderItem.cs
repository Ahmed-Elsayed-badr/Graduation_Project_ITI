using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class OrderItem
    {
        public Guid Id { get; set; } = new Guid();
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Guid SellerId { get; set; }
        [ForeignKey(nameof(Order))]
        public Guid OrderId { get; set; }
        public Order order { get; set; }
        [ForeignKey(nameof(Product))]
        public Guid productId { get; set; }
        public Product product { get; set; }
    }
}
