using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Configuration
{
    public class NetRoleConfiguration : IEntityTypeConfiguration<NetRole>
    {
        public void Configure(EntityTypeBuilder<NetRole> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.HasMany(x => x.roles).WithOne(x => x.netrole).HasForeignKey(x => x.netroleId).OnDelete(DeleteBehavior.Restrict);

        }
    }
}
