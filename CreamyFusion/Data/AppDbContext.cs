using Microsoft.EntityFrameworkCore;
using CreamyFusion.Models;

namespace CreamyFusion.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor to pass the DbContextOptions to the base class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets represent tables in your database
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductPrices)     // A Product can have many ProductPrices
                .WithOne(pp => pp.Product)        // Each ProductPrice belongs to one Product
                .HasForeignKey(pp => pp.ProductId); // Foreign key is ProductId

            // specify precision and scale for price
            modelBuilder.Entity<ProductPrice>()
                .Property(pp => pp.Price)
                .HasColumnType("decimal(18,2)"); //This sets the column to decimal with 18 digits in total and 2 decimal places
        }
    }
}
