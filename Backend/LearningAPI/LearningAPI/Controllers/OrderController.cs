using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LearningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(MyDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: Create Order
        [HttpPost, Authorize]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest createOrderRequest)
        {
            // Get the current user ID
            int userId = GetUserId();
            if (userId == 0)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Fetch the user's cart items
            var cartItems = await _context.Carts
               .Where(c => c.UserId == userId)
               .Include(c => c.Product)  // Include related Product data
               .Include(c => c.Variant)  // Include related Variant data
               .ToListAsync();

            if (!cartItems.Any())
            {
                return BadRequest("Cart is empty. Cannot create an order.");
            }

            // Create the order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending",
                DeliveryDate = createOrderRequest.DeliveryDate,
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    ProductName = c.Product.ProductName, // Get ProductName from Product
                    ImageFile = c.Product.ImageFile,
                    Size = c.Variant.Size, // Get Size from Variant
                    Quantity = c.Quantity,
                    Price = c.Variant.Price // Get Price from Variant
                }).ToList()
            };

            // Save the order to the database
            _context.Orders.Add(order);

            // Clear the user's cart
            _context.Carts.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderByOrderId), new { orderId = order.OrderId }, order);

        }


        // GET: Get Order by OrderId
        [HttpGet("detail/{orderId}")]
        public async Task<ActionResult<Order>> GetOrderByOrderId(int orderId)
        {
            int userId = GetUserId(); // Assuming this gets the authenticated user's ID

            if (userId == 0)
            {
                return Unauthorized("User is not authenticated.");
            }

            var order = await _context.Orders
                .Where(o => o.OrderId == orderId && o.UserId == userId)  // Fetch only orders for the authenticated user
                .Include(o => o.OrderItems)  // Including related OrderItems
                .FirstOrDefaultAsync();  // Get the first matching order (should be one or none)

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            return Ok(order);
        }


        // GET: Get All Orders by User
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrderById(int userId)
        {
            // You can also add a check to ensure that the authenticated user matches the userId provided in the request
            int authenticatedUserId = GetUserId(); // Assuming this gets the authenticated user's ID

            if (authenticatedUserId == 0 || authenticatedUserId != userId)
            {
                return Unauthorized("User is not authenticated or does not have permission to view these orders.");
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)  // This ensures you load the associated OrderItems
                .ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound("No orders found for this user.");
            }

            return Ok(orders);
        }

        // Verify if the user has purchased the product.
        [HttpGet("check-purchase/{productId}"), Authorize]
        public async Task<IActionResult> CheckIfUserPurchasedProduct(int productId)
        {
            try
            {
                int userId = GetUserId(); // Get the logged-in user's ID

                bool hasPurchased = await _context.Orders
                    .Include(o => o.OrderItems)
                    .AnyAsync(o => o.UserId == userId && o.OrderItems.Any(oi => oi.ProductId == productId));

                return Ok(new { hasPurchased });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking purchase status");
                return StatusCode(500, "An error occurred while checking purchase status.");
            }
        }



        private int GetUserId()
        {
            return Convert.ToInt32(User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
