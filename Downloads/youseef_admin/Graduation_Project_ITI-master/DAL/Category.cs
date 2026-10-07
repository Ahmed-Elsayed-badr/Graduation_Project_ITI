using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Category
    {
        public Guid Id { get; set; } = new Guid();
        public string Name { get; set; }
        public string Comment { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public ICollection<Product> products { get; set; }

    }
}
