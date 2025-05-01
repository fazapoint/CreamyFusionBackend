using System.Text.Json.Serialization;

namespace CreamyFusion.Models
{
    public class ProductPrice : BaseEntity
    {
        public Guid ProductId { get; set; }
        public decimal Price { get; set; }
        public DateTime ValidTo { get; set; }

        // Navigation property to the related Product (optional for api)
        public Product Product { get; set; } 
    }
}
