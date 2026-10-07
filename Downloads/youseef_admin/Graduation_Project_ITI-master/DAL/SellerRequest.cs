using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class SellerRequest
    {
        public Guid Id { get; set; } = new Guid();
        [ForeignKey(nameof(User))]
        public Guid userId { get; set; }
        public User user { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; }
        public DateTime ReviewdAt { get; set; }
        [ForeignKey(nameof(NetRole))]
        public Guid RoleId { get; set; }
        public NetRole Role { get; set; }

    }
}
