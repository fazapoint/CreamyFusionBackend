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
        public DbSet<Customer> Customers { get; set; }

        // Set base entity for updates modified or created column on every tables
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.Created = DateTime.UtcNow;
                }

                entity.Modified = DateTime.UtcNow;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure common properties for all entities that inherit from BaseEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(t => t.ClrType.IsSubclassOf(typeof(BaseEntity))))
            {
                modelBuilder.Entity(entityType.ClrType, builder =>
                {
                    builder.Property(nameof(BaseEntity.Created))
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()")
                        .IsRequired();

                    builder.Property(nameof(BaseEntity.Modified))
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()")
                        .IsRequired();
                });
            }

            modelBuilder.Entity<Product>(entity =>
            {
                // set table relations
                entity.HasMany(p => p.ProductPrices)     // A Product can have many ProductPrices
                .WithOne(pp => pp.Product)        // Each ProductPrice belongs to one Product
                .HasForeignKey(pp => pp.ProductId); // Foreign key is ProductId

                // set column name setting
                entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

                // set name unique across product table (single column)
                entity.HasIndex(p => p.Name)
                .IsUnique();
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

                entity.Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

                // Set gender into string column in sql server
                entity.Property(c => c.Gender)
                .HasConversion<string>()
                .HasMaxLength(10);

                entity.Property(c => c.Point)
                .HasDefaultValue(0);

                // set unique value for phonenumber
                entity.HasIndex(c => c.PhoneNumber)
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
