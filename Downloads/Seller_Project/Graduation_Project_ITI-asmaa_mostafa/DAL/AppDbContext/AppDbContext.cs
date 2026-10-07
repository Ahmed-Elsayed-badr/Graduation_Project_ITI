using DAL;
//using Graduation_Project_ITI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Configuration;

public class AppDbContext : DbContext
{

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Fallback only: used when the context is created without options (e.g. some design-time tooling).
    // At runtime the connection string comes from appsettings.json ("ConnectionStrings:DefaultConnection").
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //if (!optionsBuilder.IsConfigured)
        //{
            optionsBuilder.UseSqlServer("Server=DESKTOP-T5AL40F;Database=GraduationProjec_ITI_assmaa;Trusted_Connection=True;TrustServerCertificate=True");
        
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Seller> Sellers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartIItem> CartIItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<NetRole> NetRoles { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Reviews> Reviews { get; set; }
    public DbSet<SellerRequest> SellerRequests { get; set; }
    public DbSet<WishList> WishLists { get; set; }
    public DbSet<WishListItem> WishListItems { get; set; }

}
