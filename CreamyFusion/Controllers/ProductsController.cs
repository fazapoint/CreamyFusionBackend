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
                .AsNoTracking() // for read only operation no cached increase performance
                .Where(p => !p.Deleted)
                .Select(p => new ProductDto
                {
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
        public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
        {
            // Include related data if needed (e.g., for prices)
            var productDto = await _context.Products
                .AsNoTracking() // for read only operation no cached increase performance
                .Where(p => p.Id == id && !p.Deleted)
                .Select(p => new ProductDto
                {
                    Name = p.Name,
                    CurrentPrice = p.ProductPrices
                        .Where(pp => pp.ValidTo > DateTime.UtcNow)
                        .OrderByDescending(pp => pp.ValidTo)
                        .Select(pp => pp.Price)
                        .FirstOrDefault()
                }).FirstOrDefaultAsync();

            if (productDto == null)
            {
                return NotFound();
            }

            return Ok(productDto);
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(ProductInputDto inputPoductDto)
        {
            // Create a new product and map only the necessary properties
            var product = new Product
            {
                Name = inputPoductDto.Name,
                ProductPrices = new List<ProductPrice>
                {
                    new ProductPrice
                    {
                        Price = inputPoductDto.Price,
                        ValidTo = DateTime.MaxValue
                    }
                }
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // Create response with dto
            var responseDto = new ProductResponseDto
            {
                Name = product.Name,
                CurrentPrice = product.ProductPrices.First().Price,
                Message = $"ProductId: {product.Id} successfully added"
               
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, responseDto);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, ProductInputDto inputProductDto)
        {
            // get product by id
            var product = await _context.Products
                .Include(p => p.ProductPrices) // Include ProductPrices
                .FirstOrDefaultAsync(p => p.Id == id && !p.Deleted);

            if (product == null)
            {
                return NotFound();
            }

            // Update Name from DTO
            product.Name = inputProductDto.Name;

            // Find current active price
            var currentPrice = product.ProductPrices.FirstOrDefault(pp => pp.ValidTo == DateTime.MaxValue && pp.Deleted == false);

            if (currentPrice != null)
            {
                currentPrice.ValidTo = DateTime.UtcNow;
            }

            // new ProductPrice
            var newProductPrice = new ProductPrice
            {
                ProductId = id,
                Price = inputProductDto.Price,
                ValidTo = DateTime.MaxValue
            };

            // Insert into ProductPrices
            _context.ProductPrices.Add(newProductPrice);

            await _context.SaveChangesAsync();

            var responseDto = new ProductResponseDto
            {
                Name = product.Name,
                CurrentPrice = inputProductDto.Price,
                Message = $"ProductId: {product.Id} successfully updated"
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, responseDto);
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.ProductPrices)
                .FirstOrDefaultAsync(p => p.Id == id && !p.Deleted);

            if (product == null)
            {
                return NotFound();
            }

            // set deleted true for Products table
            product.Deleted = true;

            foreach (var productprices in product.ProductPrices)
            {
                // set max validto to current time
                if (productprices.ValidTo == DateTime.MaxValue){
                    productprices.ValidTo = DateTime.UtcNow;
                }
                // set all ProductPrices deleted true
                productprices.Deleted = true;
            }

            // used if want to hard delete
            //_context.Products.Remove(product);
            await _context.SaveChangesAsync();

            var responseDto = new ProductResponseDto
            {
                Name = product.Name,
                CurrentPrice = product.ProductPrices.OrderByDescending(pp => pp.ValidTo).FirstOrDefault().Price,
                Message = $"ProductId: {product.Id} successfully deleted"
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, responseDto);
        }
    }   
}
