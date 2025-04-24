using System.Text.Json.Serialization;

namespace CreamyFusion.Models
{
    public class ProductPrice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public decimal Price { get; set; }
        public DateTime ValidTo { get; set; }
        public bool Deleted { get; set; }

        // Navigation property to the related Product (optional for api)
        public Product Product { get; set; } 
    }
}
