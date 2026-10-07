using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(c => c.Name).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.HasMany(x => x.order).WithOne(x => x.user).HasForeignKey(x => x.userId)
                                         .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.role).WithOne(x => x.user).HasForeignKey(x => x.userId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.cart).WithOne(x => x.user).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.WishList).WithOne(x => x.User).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.reviews).WithOne(x => x.user).HasForeignKey(x => x.userId).OnDelete(DeleteBehavior.Cascade);


        }
    }
}
