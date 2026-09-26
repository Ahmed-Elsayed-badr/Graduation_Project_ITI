using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class NetRole
    {
        public Guid Id  { get; set; } = new Guid();
        public string NormalizedName { get; set; } = string.Empty;
        public ICollection<Role> roles { get; set; } 
    }
}
