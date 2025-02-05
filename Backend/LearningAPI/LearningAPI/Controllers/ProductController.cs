//using AutoMapper;
//using LearningAPI.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
//using Microsoft.Extensions.Logging;

//namespace LearningAPI.Controllers
//{
//	[ApiController]
//	[Route("[controller]")]
//	public class ProductController(MyDbContext context, IMapper mapper,
//		ILogger<ProductController> logger) : ControllerBase
//	{
//		[HttpGet]
//		[ProducesResponseType(typeof(IEnumerable<ProductDTO>), StatusCodes.Status200OK)]
//		public async Task<IActionResult> GetAll(string? search)
//		{
//			try
//			{
//				IQueryable<Product> result = context.Products.Include(t => t.User);
//				if (search != null)
//				{
//					result = result.Where(x => x.ProductName.Contains(search)
//						|| x.Description.Contains(search));
//				}
//				var list = await result.OrderByDescending(x => x.CreatedAt).ToListAsync();
//				IEnumerable<ProductDTO> data = list.Select(mapper.Map<ProductDTO>);
//				return Ok(data);
//			}
//			catch (Exception ex)
//			{
//				logger.LogError(ex, "Error when get all products");
//				return StatusCode(500);
//			}
//		}

//		[HttpGet("{id}")]
//		[ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
//		public async Task<IActionResult> GetProduct(int id)
//		{
//			try
//			{
//				Product? product = await context.Products.Include(t => t.User)
//				.SingleOrDefaultAsync(t => t.Id == id);
//				if (product == null)
//				{
//					return NotFound();
//				}
//				ProductDTO data = mapper.Map<ProductDTO>(product);
//				return Ok(data);
//			}
//			catch (Exception ex)
//			{
//				logger.LogError(ex, "Error when get product by id");
//				return StatusCode(500);
//			}
//		}

//		[HttpPost, Authorize]
//		[ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
//		public async Task<IActionResult> AddProduct(AddProductRequest product)
//		{
//			try
//			{
//				int userId = GetUserId();
//				var now = DateTime.Now;
//				var newProduct = new Product
//				{
//					ProductName = product.ProductName.Trim(),
//					Description = product.Description.Trim(),
//					ImageFile = product.ImageFile,
//					IsArchived = product.IsArchived,
//					CreatedAt = DateTime.Now,
//					UpdatedAt = DateTime.Now,
//					UserId = userId
//				};

//				//_logger.LogInformation{ Description}

//				await context.Products.AddAsync(newProduct);
//				await context.SaveChangesAsync();

//				Product? addedProduct = context.Products.Include(t => t.User)
//					.FirstOrDefault(t => t.Id == newProduct.Id);
//				ProductDTO productDTO = mapper.Map<ProductDTO>(newProduct);
//				return Ok(productDTO);
//			}
//			catch (Exception ex)
//			{
//				logger.LogError(ex, "Error when adding product");
//				return StatusCode(500);
//			}
//		}

//		[HttpPut("{id}"), Authorize]
//		public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest product)
//		{
//			try
//			{
//				var myProduct = await context.Products.FindAsync(id);
//				if (myProduct == null)
//				{
//					return NotFound();
//				}

//				int userId = GetUserId();
//				if (myProduct.UserId != userId)
//				{
//					return Forbid();
//				}

//				if (product.ProductName != null)
//				{
//					myProduct.ProductName = product.ProductName.Trim();
//				}
//				if (product.Description != null)
//				{
//					myProduct.Description = product.Description.Trim();
//				}
//				if (product.ImageFile != null)
//				{
//					myProduct.ImageFile = product.ImageFile.Trim();
//				}
//				myProduct.IsArchived = product.IsArchived;
//				myProduct.UpdatedAt = DateTime.Now;

//				await context.SaveChangesAsync();
//				return Ok();
//			}
//			catch (Exception ex)
//			{
//				logger.LogError(ex, "Error when updating product");
//				return StatusCode(500);
//			}
//		}

//		[HttpDelete("{id}"), Authorize]
//		public async Task<IActionResult> DeleteProduct(int id)
//		{
//			try
//			{
//				var myProduct = context.Products.Find(id);
//				if (myProduct == null)
//				{
//					return NotFound();
//				}

//				int userId = GetUserId();
//				if (myProduct.UserId != userId)
//				{
//					return Forbid();
//				}

//				context.Products.Remove(myProduct);
//				await context.SaveChangesAsync();
//				return Ok();
//			}
//			catch (Exception ex)
//			{
//				logger.LogError(ex, "Error when deleting product");
//				return StatusCode(500);
//			}
//		}

//		private int GetUserId()
//		{
//			return Convert.ToInt32(User.Claims
//				.Where(c => c.Type == ClaimTypes.NameIdentifier)
//				.Select(c => c.Value).SingleOrDefault());
//		}
//	}
//}

using AutoMapper;
using System.Security.Claims;
using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
				var product = await _context.Products.SingleOrDefaultAsync(p => p.Id == id && p.UserId == userId);
				if (product == null)
				{
					return NotFound("Product not found or does not belong to the user.");
				}

				// Update the product fields if provided in the request
				if (!string.IsNullOrEmpty(request.ProductName))
				{
					product.ProductName = request.ProductName.Trim();
				}
				if (!string.IsNullOrEmpty(request.Description))
				{
					product.Description = request.Description.Trim();
				}
				if (!string.IsNullOrEmpty(request.ImageFile))
				{
					product.ImageFile = request.ImageFile.Trim();
				}
				if (request.IsArchived.HasValue)
				{
					product.IsArchived = request.IsArchived.Value;
				}

				// Update the last modified time
				product.UpdatedAt = DateTime.UtcNow;

				// Save the changes
				await _context.SaveChangesAsync();

				// Convert to DTO and return response
				var productDTO = _mapper.Map<ProductDTO>(product);
				return Ok(productDTO);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while updating product");
				return StatusCode(500, "An error occurred while updating the product.");
			}
		}

		// Add Product
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

				// Validation: User Exists
				bool userExists = await _context.Users.AnyAsync(u => u.Id == userId);
				if (!userExists)
				{
					return BadRequest("Invalid UserId. The user does not exist.");
				}

				// Create a new product object
				var now = DateTime.UtcNow;
				var product = new Product
				{
					ProductName = request.ProductName.Trim(),
					Description = request.Description.Trim(),
					ImageFile = request.ImageFile,
					IsArchived = request.IsArchived,
					CreatedAt = now,
					UpdatedAt = now,
					UserId = userId
				};

				// Save product to the database
				await _context.Products.AddAsync(product);
				await _context.SaveChangesAsync();

				// Convert to DTO and return response
				var productDTO = _mapper.Map<ProductDTO>(product);
				return Ok(productDTO);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while adding product");
				return StatusCode(500, "An error occurred while saving the product.");
			}
		}

		// Get All Products for the Logged-in User

		[HttpGet("my-products"), Authorize]
		[ProducesResponseType(typeof(IEnumerable<ProductDTO>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetMyProducts(int? id = null)
		{
			try
			{
				// Get UserId
				int userId = GetUserId();

				// Base query to get products for the logged-in user
				IQueryable<Product> query = _context.Products
					.Where(p => p.UserId == userId);

				// If id is provided, filter by id
				if (id.HasValue)
				{
					query = query.Where(p => p.Id == id.Value);
				}

				// Retrieve and order products
				var products = await query
					.OrderBy(p => p.UpdatedAt) // Sort by UpdatedAt (Ascending order)
					.ToListAsync();

				// Map products to DTOs
				var productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(products);

				return Ok(productDTOs);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while retrieving user products");
				return StatusCode(500, "An error occurred while retrieving products.");
			}
		}

		[HttpGet("product/{id}"), Authorize]
		[ProducesResponseType(typeof(ProductDTO), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetProductById(int id)
		{
			try
			{
				// Get the product by ID
				var product = await _context.Products
											.Where(p => p.UserId == GetUserId())
											.SingleOrDefaultAsync(p => p.Id == id);

				if (product == null)
				{
					return NotFound("Product not found");
				}

				// Map the product to DTO
				var productDTO = _mapper.Map<ProductDTO>(product);
				return Ok(productDTO);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while retrieving product with ID {Id}", id);
				return StatusCode(500, "An error occurred while retrieving the product.");
			}
		}


		// Delete Product
		[HttpDelete("{id}"), Authorize]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			try
			{
				// Get UserId for validation
				int userId = GetUserId();

				// Find the product and validate ownership
				var product = await _context.Products.SingleOrDefaultAsync(p => p.Id == id && p.UserId == userId);
				if (product == null)
				{
					return NotFound("Product not found or does not belong to the user.");
				}

				// Delete product
				_context.Products.Remove(product);
				await _context.SaveChangesAsync();

				return Ok("Product deleted successfully.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while deleting product");
				return StatusCode(500, "An error occurred while deleting the product.");
			}
		}

		// Helper: Get UserId from Claims
		private int GetUserId()
		{
			//return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
			return 1;
		}
	}
}