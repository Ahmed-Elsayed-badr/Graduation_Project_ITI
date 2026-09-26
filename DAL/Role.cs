using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Role
    {
       public Guid Id { get; set; } = new Guid();
       public string Name { get; set; }

        //public string NormalizeName { get; set; }
        [ForeignKey(nameof(User))]
       public Guid userId { get; set; }
       public User user { get; set; }
        public ICollection<Product> Products { get; set; }
        public ICollection<NetRole> roles { get; set; }
        [ForeignKey(nameof(NetRole))]
        public Guid netroleId { get; set; }
        public NetRole netrole { get; set; }

 
    }
}
