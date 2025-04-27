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

            modelBuilder.Entity<Product>(entity =>
            {
                // set table relations
                entity.HasMany(p => p.ProductPrices)     // A Product can have many ProductPrices
                .WithOne(pp => pp.Product)        // Each ProductPrice belongs to one Product
                .HasForeignKey(pp => pp.ProductId); // Foreign key is ProductId

                // set column name
                entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

                // set name unique across product table (single column)
                entity.HasIndex(p => p.Name)
                .IsUnique();
            });


            // specify precision and scale for price
            modelBuilder.Entity<ProductPrice>(entity =>
            {
                entity.Property(pp => pp.Price)
                .HasColumnType("decimal(18,2)"); //This sets the column to decimal with 18 digits in total and 2 decimal places

                // set composite key should be unique (multiple column that is productid and validto)
                entity.HasIndex(pp => new { pp.ProductId, pp.ValidTo })
                .IsUnique();
            });
                
        }
    }
}
