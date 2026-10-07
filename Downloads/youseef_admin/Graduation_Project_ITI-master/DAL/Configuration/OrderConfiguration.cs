using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Configuration
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.HasOne(x => x.user).WithMany(x => x.order).HasForeignKey(x => x.userId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.OrderItems).WithOne(x => x.order).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);

        }
    }
}
