using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CreamyFusion.Data;
using CreamyFusion.Models;
using CreamyFusion.DTOs;

namespace CreamyFusion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var productDtos = await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CurrentPrice = p.ProductPrices
                        .Where(pp => pp.ValidTo > DateTime.UtcNow) // validto > todaytime
                        .OrderByDescending(pp => pp.ValidTo) // most recent valid
                        .Select(pp => pp.Price)
                        .FirstOrDefault()
                }).ToListAsync();

            return Ok(productDtos);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            // var product = await _context.Products.SingleOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(ProductInputDto productDto)
        {
            // Create a new product and map only the necessary properties
            var product = new Product
            {
                Name = productDto.Name,
                ProductPrices = new List<ProductPrice>
                {
                    new ProductPrice
                    {
                        Price = productDto.InitialPrice,
                        ValidTo = DateTime.MaxValue
                    }
                }
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // Create response with dto
            var responseDto = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                CurrentPrice = product.ProductPrices.First().Price
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, responseDto);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(Guid id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }   
}
