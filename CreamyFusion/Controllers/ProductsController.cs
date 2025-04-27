using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CreamyFusion.Data;
using CreamyFusion.Models;
using CreamyFusion.DTOs;
using Microsoft.Data.SqlClient;

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
        public async Task<ActionResult<Product>> CreateProduct(ProductInputDto inputProductDto)
        {
            try
            {
                // Create a new product and map only the necessary properties
                var product = new Product
                {
                    Name = inputProductDto.Name,
                    ProductPrices = new List<ProductPrice>
            {
                new ProductPrice
                {
                    Price = inputProductDto.Price,
                    ValidTo = DateTime.MaxValue
                }
            }
                };

                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                // Create response with DTO
                var responseDto = new ProductResponseDto
                {
                    Name = product.Name,
                    CurrentPrice = product.ProductPrices.First().Price,
                    Message = $"ProductId: {product.Id} successfully added"
                };

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, responseDto);
            }
            catch (DbUpdateException dbEx)
            {
                // Log or inspect the exception to check what the inner exception type is
                var innerException = dbEx.InnerException as SqlException;
                if (innerException != null)
                {
                    return StatusCode(500, new
                    {
                        message = "An error occurred while saving the product (sql inner exception).",
                        exceptionMessage = innerException.Message
                    });
                }

                // In case it's not an SQL exception, return a generic DB update error
                return StatusCode(500, new
                {
                    message = "An error occurred while saving the product (not sql exception)",
                    exceptionMessage = dbEx.Message
                });
            }
            catch (Exception ex)
            {
                // Catch any other general exception
                return StatusCode(500, new
                {
                    message = "An error occurred while adding the product (general exception)",
                    exceptionMessage = ex.Message
                });
            }
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
                // if currentprice is null then output 0
                CurrentPrice = product.ProductPrices.FirstOrDefault(pp => pp.ValidTo == DateTime.MaxValue)?.Price ?? 0m,
                Message = $"ProductId: {product.Id} successfully deleted"
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, responseDto);
        }
    }   
}
