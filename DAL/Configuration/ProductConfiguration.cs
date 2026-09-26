using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.HasOne(x => x.Role).WithMany(x => x.Products).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.orderItems).WithOne(x => x.product).HasForeignKey(x =>x.productId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.category).WithMany(x => x.products).HasForeignKey(x => x.categoryId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Seller).WithMany(x => x.Products).HasForeignKey(x => x.sellerId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.cartItems).WithOne(x => x.product).HasForeignKey(x => x.productId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.reviews).WithOne(x => x.Product).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);   
            
        }
    }
}
