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

				// Admin can archive/unarchive any product
				if (User.IsInRole("Admin") && request.IsArchived.HasValue)
				{
					product.IsArchived = request.IsArchived.Value;
				}

				// Ensure product is archived when all variants have stock = 0
				if (product.Variants != null && product.Variants.All(v => v.Stock <= 0))
				{
					product.IsArchived = true;
				}
				else
				{
					product.IsArchived = false; // Unarchive if at least one variant has stock
				}

				// If stock is added, unarchive the product (unless admin manually archived it)
				if (request.Variants != null && request.Variants.Any(v => v.Stock > 0))
				{
					// Only unarchive if the product was not manually archived by Admin
					if (product.IsArchived.HasValue && product.IsArchived.Value == false)
					{
						product.IsArchived = false;
					}
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
					CategoryGender = request.CategoryGender.Trim(),
					Reviews = new List<Review>()
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

        [HttpGet("product/{id}")]
        [AllowAnonymous] // ✅ Allow everyone to view product details
        [ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                    .Include(p => p.User) // Ensure we fetch product owner details
                    .ThenInclude(u => u.UserRole)
                    .SingleOrDefaultAsync(p =>
                        p.Id == id &&
                        (!p.IsArchived.HasValue || p.IsArchived == false) && // Ensure product is not archived
                        (User.IsInRole("Admin") || p.User.UserRole.Role == "Admin" || p.UserId == GetUserId()));

                if (product == null)
                {
                    return NotFound("Product not found.");
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

		//[HttpGet("admin"), Authorize(Roles = "Admin")]
		[HttpGet("admin"), Authorize]
		[ProducesResponseType(typeof(IEnumerable<ProductDTO>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetAllProductsForAdmin()
		{
			try
			{
				var products = await _context.Products
					.Include(p => p.Variants)
					.Include(p => p.Reviews)
					.Include(p => p.User)
					.OrderBy(p => p.UpdatedAt)
					.ToListAsync();

				// Ensure isArchived is updated correctly before sending response
				foreach (var product in products)
				{
					if (product.Variants.All(v => v.Stock == 0))
					{
						product.IsArchived = true;
					}
					else
					{
						product.IsArchived = false;
					}
				}

				await _context.SaveChangesAsync(); // Persist changes

				var productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(products);
				return Ok(productDTOs);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while retrieving all products for admin");
				return StatusCode(500, "An error occurred while retrieving products.");
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
					.Include(p => p.Reviews)
					.Where(p => p.UserId == userId && p.IsArchived == false)
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

		[HttpGet("public-products")]
		[AllowAnonymous] // Allows customers and guests to see products
		[ProducesResponseType(typeof(IEnumerable<ProductDTO>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetProductsPostedByAdmins()
		{
			try
			{
				var adminProducts = await _context.Products
					.Include(p => p.Variants)
					.Include(p => p.Reviews)
					.Include(p => p.User)
					.ThenInclude(u => u.UserRole) // Ensure UserRole is loaded
					.Where(p => p.IsArchived == false && p.User.UserRole != null && p.User.UserRole.Role == "Admin")
					.OrderByDescending(p => p.UpdatedAt)
					.ToListAsync();

				var productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(adminProducts);
				return Ok(productDTOs);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while retrieving admin products");
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
					.Include(p => p.Reviews)
					.SingleOrDefaultAsync(p => p.Id == id && p.UserId == userId);

				if (product == null)
				{
					return NotFound("Product not found or does not belong to the user.");
				}

				// Delete associated reviews first
				if (product.Reviews != null && product.Reviews.Any())
				{
					_context.Reviews.RemoveRange(product.Reviews);
				}

				// Delete associated variants
				if (product.Variants != null && product.Variants.Any())
				{
					_context.Variants.RemoveRange(product.Variants);
				}

				// Remove the product
				_context.Products.Remove(product);

				await _context.SaveChangesAsync();

                return Ok("Product and associated reviews/variants deleted successfully.");
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
        [HttpPut("update-stock/{productId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStock(int productId, [FromBody] List<UpdateStockRequest> request)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .SingleOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    return NotFound("Product not found.");
                }


                if (product.Variants == null)
                {
                    product.Variants = new List<Variant>();
                }


                foreach (var stockUpdate in request)
                {
                    var variant = product.Variants.FirstOrDefault(v => v.VariantId == stockUpdate.VariantId);
                    if (variant != null)
                    {
                        variant.Stock = stockUpdate.Stock;
                    }
                    else
                    {
                        return BadRequest($"Variant with ID {stockUpdate.VariantId} not found.");
                    }
                }
                product.UpdatedAt = DateTime.UtcNow;


                await _context.SaveChangesAsync();

                return Ok("Stock updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating stock");
                return StatusCode(500, "An error occurred while updating stock.");
            }
        }
    }
}