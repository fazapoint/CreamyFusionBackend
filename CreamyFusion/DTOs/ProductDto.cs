namespace CreamyFusion.DTOs
{
    public class ProductDto
    {
        public string Name { get; set; }
        public decimal CurrentPrice { get; set; }
    }

    public class ProductInputDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductResponseDto
    {
        public string Name { get; set; }
        public decimal CurrentPrice { get; set; }
    }
}
