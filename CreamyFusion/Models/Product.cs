namespace CreamyFusion.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }

        // Navigation property for the one-to-many relationship with ProductPrices
        public ICollection<ProductPrice> ProductPrices { get; set; }
    }
}
