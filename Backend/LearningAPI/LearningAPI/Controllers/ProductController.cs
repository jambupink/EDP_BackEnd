using AutoMapper;
using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController(MyDbContext context, IMapper mapper,
        ILogger<ProductController> logger) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(string? search)
        {
            try
            {
                IQueryable<Product> result = context.Products.Include(t => t.User);
                if (search != null)
                {
                    result = result.Where(x => x.Title.Contains(search)
                        || x.Description.Contains(search));
                }
                var list = await result.OrderByDescending(x => x.CreatedAt).ToListAsync();
                IEnumerable<ProductDTO> data = list.Select(mapper.Map<ProductDTO>);
                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when get all products");
                return StatusCode(500);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
				Product? product = await context.Products.Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Id == id);
                if (product == null)
                {
                    return NotFound();
                }
				ProductDTO data = mapper.Map<ProductDTO>(product);
                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when get product by id");
                return StatusCode(500);
            }
        }

        [HttpPost, Authorize]
        [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddProduct(AddProductRequest product)
        {
            try
            {
                int userId = GetUserId();
                var now = DateTime.Now;
                var myProduct = new Product()
                {
                    Title = product.Title.Trim(),
                    Description = product.Description.Trim(),
                    ImageFile = product.ImageFile,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UserId = userId
                };

                await context.Products.AddAsync(myProduct);
                await context.SaveChangesAsync();

				Product? newProduct = context.Products.Include(t => t.User)
                    .FirstOrDefault(t => t.Id == myProduct.Id);
				ProductDTO productDTO = mapper.Map<ProductDTO>(newProduct);
                return Ok(productDTO);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when add product");
                return StatusCode(500);
            }
        }

        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest product)
        {
            try
            {
                var myProduct = await context.Products.FindAsync(id);
                if (myProduct == null)
                {
                    return NotFound();
                }

                int userId = GetUserId();
                if (myProduct.UserId != userId)
                {
                    return Forbid();
                }

                if (product.Title != null)
                {
					myProduct.Title = product.Title.Trim();
                }
                if (product.Description != null)
                {
					myProduct.Description = product.Description.Trim();
                }
                if (product.ImageFile != null)
                {
					myProduct.ImageFile = product.ImageFile;
                }
				myProduct.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when update product");
                return StatusCode(500);
            }
        }

        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var myProduct = context.Products.Find(id);
                if (myProduct == null)
                {
                    return NotFound();
                }

                int userId = GetUserId();
                if (myProduct.UserId != userId)
                {
                    return Forbid();
                }

                context.Products.Remove(myProduct);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when delete product");
                return StatusCode(500);
            }
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value).SingleOrDefault());
        }
    }
}
