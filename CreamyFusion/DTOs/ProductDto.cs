namespace CreamyFusion.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal CurrentPrice { get; set; }
    }

    public class ProductInputDto
    {
        public string Name { get; set; }
        public decimal InitialPrice { get; set; }
    }

    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal CurrentPrice { get; set; }
    }
}
