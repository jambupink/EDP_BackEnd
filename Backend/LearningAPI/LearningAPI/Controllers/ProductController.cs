//using AutoMapper;
//using System.Security.Claims;
//using LearningAPI.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace LearningAPI.Controllers
//{
//	[ApiController]
//	[Route("[controller]")]
//	public class ProductController : ControllerBase
//	{
//		private readonly MyDbContext _context;
//		private readonly IMapper _mapper;
//		private readonly ILogger<ProductController> _logger;

//		public ProductController(MyDbContext context, IMapper mapper, ILogger<ProductController> logger)
//		{
//			_context = context;
//			_mapper = mapper;
//			_logger = logger;
//		}

//		[HttpPut("{id}"), Authorize]
//		[ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
//		[ProducesResponseType(StatusCodes.Status404NotFound)]
//		[ProducesResponseType(StatusCodes.Status403Forbidden)]
//		public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
//		{
//			try
//			{
//				// Get the logged-in user's ID
//				int userId = GetUserId();

//				// Find the product and validate ownership
//				var product = await _context.Products.SingleOrDefaultAsync(p => p.Id == id && p.UserId == userId);
//				if (product == null)
//				{
//					return NotFound("Product not found or does not belong to the user.");
//				}

//				// Update the product fields if provided in the request
//				if (!string.IsNullOrEmpty(request.ProductName))
//				{
//					product.ProductName = request.ProductName.Trim();
//				}
//				if (!string.IsNullOrEmpty(request.Description))
//				{
//					product.Description = request.Description.Trim();
//				}
//				if (!string.IsNullOrEmpty(request.ImageFile))
//				{
//					product.ImageFile = request.ImageFile.Trim();
//				}
//				if (request.IsArchived.HasValue)
//				{
//					product.IsArchived = request.IsArchived.Value;
//				}

//				// Update the last modified time
//				product.UpdatedAt = DateTime.UtcNow;

//				// Save the changes
//				await _context.SaveChangesAsync();

//				// Convert to DTO and return response
//				var productDTO = _mapper.Map<ProductDTO>(product);
//				return Ok(productDTO);
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Error while updating product");
//				return StatusCode(500, "An error occurred while updating the product.");
//			}
//		}

//		// Add Product
//		[HttpPost, Authorize]
//		[ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
//		public async Task<IActionResult> AddProduct(AddProductRequest request)
//		{
//			try
//			{
//				// Get UserId
//				int userId = GetUserId();

//				_logger.LogInformation($"UserId used for product: {userId}");
//				_logger.LogInformation($"ProductName received: {request.ProductName}");

//				// Validation: User Exists
//				bool userExists = await _context.Users.AnyAsync(u => u.Id == userId);
//				if (!userExists)
//				{
//					return BadRequest("Invalid UserId. The user does not exist.");
//				}

//				// Create a new product object
//				var now = DateTime.UtcNow;
//				var product = new Product
//				{
//					ProductName = request.ProductName.Trim(),
//					Description = request.Description.Trim(),
//					ImageFile = request.ImageFile,
//					IsArchived = request.IsArchived,
//					CreatedAt = now,
//					UpdatedAt = now,
//					UserId = userId
//				};

//				// Save product to the database
//				await _context.Products.AddAsync(product);
//				await _context.SaveChangesAsync();

//				// Convert to DTO and return response
//				var productDTO = _mapper.Map<ProductDTO>(product);
//				return Ok(productDTO);
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Error while adding product");
//				return StatusCode(500, "An error occurred while saving the product.");
//			}
//		}

//		// Get All Products for the Logged-in User

//		[HttpGet("my-products"), Authorize]
//		[ProducesResponseType(typeof(IEnumerable<ProductDTO>), StatusCodes.Status200OK)]
//		public async Task<IActionResult> GetMyProducts(int? id = null)
//		{
//			try
//			{
//				// Get UserId
//				int userId = GetUserId();

//				// Base query to get products for the logged-in user
//				IQueryable<Product> query = _context.Products
//					.Where(p => p.UserId == userId);

//				// If id is provided, filter by id
//				if (id.HasValue)
//				{
//					query = query.Where(p => p.Id == id.Value);
//				}

//				// Retrieve and order products
//				var products = await query
//					.OrderBy(p => p.UpdatedAt) // Sort by UpdatedAt (Ascending order)
//					.ToListAsync();

//				// Map products to DTOs
//				var productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(products);

//				return Ok(productDTOs);
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Error while retrieving user products");
//				return StatusCode(500, "An error occurred while retrieving products.");
//			}
//		}

//		[HttpGet("product/{id}"), Authorize]
//		[ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
//		[ProducesResponseType(StatusCodes.Status404NotFound)]
//		public async Task<IActionResult> GetProductById(int id)
//		{
//			try
//			{
//				// Get the product by ID
//				var product = await _context.Products
//											.Where(p => p.UserId == GetUserId())
//											.SingleOrDefaultAsync(p => p.Id == id);

//				if (product == null)
//				{
//					return NotFound("Product not found");
//				}

//				// Map the product to DTO
//				var productDTO = _mapper.Map<ProductDTO>(product);
//				return Ok(productDTO);
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Error while retrieving product with ID {Id}", id);
//				return StatusCode(500, "An error occurred while retrieving the product.");
//			}
//		}


//		// Delete Product
//		[HttpDelete("{id}"), Authorize]
//		[ProducesResponseType(StatusCodes.Status200OK)]
//		[ProducesResponseType(StatusCodes.Status404NotFound)]
//		public async Task<IActionResult> DeleteProduct(int id)
//		{
//			try
//			{
//				// Get UserId for validation
//				int userId = GetUserId();

//				// Find the product and validate ownership
//				var product = await _context.Products.SingleOrDefaultAsync(p => p.Id == id && p.UserId == userId);
//				if (product == null)
//				{
//					return NotFound("Product not found or does not belong to the user.");
//				}

//				// Delete product
//				_context.Products.Remove(product);
//				await _context.SaveChangesAsync();

//				return Ok("Product deleted successfully.");
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Error while deleting product");
//				return StatusCode(500, "An error occurred while deleting the product.");
//			}
//		}

//		// Helper: Get UserId from Claims
//		private int GetUserId()
//		{
//			//return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
//			return 1;
//		}
//	}
//}

using AutoMapper;
using System.Security.Claims;
using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LearningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductController> _logger;

        public ProductController(MyDbContext context, IMapper mapper, ILogger<ProductController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // Update Product to support multiple variants
        [HttpPut("{id}"), Authorize]
        [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                // Get the logged-in user's ID
                int userId = GetUserId();

                // Find the product and validate ownership
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .SingleOrDefaultAsync(p => p.Id == id && p.UserId == userId);

                if (product == null)
                {
                    return NotFound("Product not found or does not belong to the user.");
                }

                // Update product fields
                if (!string.IsNullOrEmpty(request.ProductName)) product.ProductName = request.ProductName.Trim();
                if (!string.IsNullOrEmpty(request.Description)) product.Description = request.Description.Trim();
                if (!string.IsNullOrEmpty(request.ImageFile)) product.ImageFile = request.ImageFile.Trim();
                if (request.IsArchived.HasValue) product.IsArchived = request.IsArchived.Value;
                // Update CategoryName and CategoryGender
                if (!string.IsNullOrEmpty(request.CategoryName)) product.CategoryName = request.CategoryName.Trim();
                if (!string.IsNullOrEmpty(request.CategoryGender)) product.CategoryGender = request.CategoryGender.Trim();

                // Ensure product.Variants is initialized
                if (product.Variants == null)
                {
                    product.Variants = new List<Variant>();
                }

                // Handle Variants update
                if (request.Variants != null)
                {
                    var existingVariantIds = product.Variants.Select(v => v.VariantId).ToList();
                    var updatedVariantIds = request.Variants.Where(v => v.VariantId.HasValue).Select(v => v.VariantId.Value).ToList();

                    // Remove variants that are **not in the updated list**
                    var variantsToRemove = product.Variants.Where(v => !updatedVariantIds.Contains(v.VariantId)).ToList();
                    if (variantsToRemove.Any()) _context.Variants.RemoveRange(variantsToRemove);

                    // Update existing variants
                    foreach (var variantDTO in request.Variants.Where(v => v.VariantId.HasValue))
                    {
                        var variant = product.Variants.FirstOrDefault(v => v.VariantId == variantDTO.VariantId);
                        if (variant != null)
                        {
                            variant.Color = variantDTO.Color;
                            variant.Size = variantDTO.Size;
                            variant.Price = variantDTO.Price;
                            variant.Stock = variantDTO.Stock;
                        }
                    }

                    // Add new variants
                    foreach (var variantDTO in request.Variants.Where(v => !v.VariantId.HasValue))
                    {
                        var newVariant = new Variant
                        {
                            ProductId = product.Id,
                            Color = variantDTO.Color,
                            Size = variantDTO.Size,
                            Price = variantDTO.Price,
                            Stock = variantDTO.Stock
                        };
                        _context.Variants.Add(newVariant);
                    }
                }

                // Update last modified timestamp
                product.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _context.SaveChangesAsync();

                // Convert to DTO and return
                var productDTO = _mapper.Map<ProductDTO>(product);
                return Ok(productDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating product");
                return StatusCode(500, "An error occurred while updating the product.");
            }
        }

        [HttpPost, Authorize]
        [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddProduct(AddProductRequest request)
        {
            try
            {
                // Get UserId
                int userId = GetUserId();

                _logger.LogInformation($"UserId used for product: {userId}");
                _logger.LogInformation($"ProductName received: {request.ProductName}");

                // Validate user
                bool userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists) return BadRequest("Invalid UserId. The user does not exist.");

                // Create new product (WITHOUT Variants first)
                var now = DateTime.UtcNow;
                var product = new Product
                {
                    ProductName = request.ProductName.Trim(),
                    Description = request.Description.Trim(),
                    ImageFile = request.ImageFile,
                    IsArchived = request.IsArchived,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UserId = userId,
                    CategoryName = request.CategoryName.Trim(),
                    CategoryGender = request.CategoryGender.Trim()
                };

                // Save the product FIRST (so that it gets an ID)
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product ID: {product.Id} successfully saved.");

                // Save multiple variants (AFTER product is saved)
                if (request.Variants != null && request.Variants.Any())
                {
                    var variants = request.Variants.Select(variantDTO => new Variant
                    {
                        ProductId = product.Id, // Now the ProductId exists
                        Color = variantDTO.Color.Trim(),
                        Size = variantDTO.Size.Trim(),
                        Price = variantDTO.Price,
                        Stock = variantDTO.Stock
                    }).ToList();

                    await _context.Variants.AddRangeAsync(variants);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Variants successfully added to product {product.Id}");
                }

                // Convert to DTO and return response
                var productDTO = _mapper.Map<ProductDTO>(product);
                return Ok(productDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding product");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("product/{id}"), Authorize]
        [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                    //.Where(p => p.UserId == GetUserId())
                    .SingleOrDefaultAsync(p => p.Id == id && p.UserId == GetUserId());

                if (product == null)
                {
                    return NotFound("Product not found");
                }

                var productDTO = _mapper.Map<ProductDTO>(product);
                return Ok(productDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving product with ID {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the product.");
            }
        }

        [HttpGet("my-products"), Authorize]
        [ProducesResponseType(typeof(IEnumerable<ProductDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyProducts()
        {
            try
            {
                int userId = GetUserId();

                var products = await _context.Products
                    .Include(p => p.Variants)
                    .Where(p => p.UserId == userId)
                    .OrderBy(p => p.UpdatedAt)
                    .ToListAsync();

                var productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(products);
                return Ok(productDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving user products");
                return StatusCode(500, "An error occurred while retrieving products.");
            }
        }

        [HttpDelete("{id}"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                int userId = GetUserId();

                // Fetch the product along with its variants
                var product = await _context.Products
                    .Include(p => p.Variants) // Include variants to delete them first
                    .SingleOrDefaultAsync(p => p.Id == id && p.UserId == userId);

                if (product == null)
                {
                    return NotFound("Product not found or does not belong to the user.");
                }

                // Ensure `Variants` is not null before removing them
                if (product.Variants != null && product.Variants.Any())
                {
                    _context.Variants.RemoveRange(product.Variants);
                }

                // Remove the product
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();

                return Ok("Product and associated variants deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting product");
                return StatusCode(500, "An error occurred while deleting the product.");
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