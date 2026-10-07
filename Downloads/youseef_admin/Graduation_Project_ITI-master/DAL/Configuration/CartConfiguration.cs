using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Configuration
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
              .ValueGeneratedOnAdd();
            builder.HasOne(x => x.user).WithOne(x => x.cart).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.cartIItems).WithOne(x => x.cart).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);

        }
    }
}
