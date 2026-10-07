using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Admin
    {
        public Guid Id { get; set; } = new Guid();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<Seller> sellers { get; set; }
        public ICollection<User> users { get; set; }
    }
}
