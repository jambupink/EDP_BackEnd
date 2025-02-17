using LearningAPI.DTOs;
using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(MyDbContext context, ILogger<ReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Add a Review
        [HttpPost, Authorize]
        public async Task<IActionResult> AddReview([FromBody] AddReviewRequest request)
        {
            try
            {
                int userId = GetUserId(); // Get authenticated user ID

                // 🔹 Ensure the order exists and belongs to the user before allowing the review
                bool validOrder = await _context.Orders
                    .AnyAsync(o => o.OrderId == request.OrderId && o.UserId == userId);

                if (!validOrder)
                {
                    return BadRequest("You can only review products that you have purchased.");
                }

                var review = new Review
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    OrderId = request.OrderId, // 🔹 Link review to order
                    Comments = request.Comments.Trim(),
                    Rating = Math.Round(request.Rating, 1), // Ensure 1 decimal place
                    ReviewDate = DateTime.UtcNow
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding review.");
                return StatusCode(500, "An error occurred while adding the review.");
            }
        }

        // Get Reviews for a Product
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetReviewsByProduct(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            if (reviews == null || reviews.Count == 0)
                return NotFound("No reviews found for this product.");

            var reviewDTOs = reviews.Select(r => new ReviewDTO
            {
                ReviewId = r.ReviewId,
                Comments = r.Comments,
                Rating = r.Rating,
                ReviewDate = r.ReviewDate,
                UserId = r.UserId,
                UserName = r.User != null ? r.User.Name : "Anonymous"
            }).ToList();

            return Ok(reviewDTOs);
        }


        // Get a Specific Review
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .SingleOrDefaultAsync(r => r.ReviewId == id);

            if (review == null)
                return NotFound("Review not found.");

            return Ok(review);
        }

        // Update a Review (Only Owner Can Edit)
        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewRequest request)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);

                if (review == null)
                    return NotFound("Review not found.");

                if (review.UserId != GetUserId()) // Ensure only the review owner can update
                    return Forbid();

                // Update only Comments and Rating
                review.Comments = request.Comments.Trim();
                review.Rating = Math.Round(request.Rating, 1); // Ensure 1 decimal place
                review.ReviewDate = DateTime.UtcNow; // Set update timestamp

                await _context.SaveChangesAsync();
                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating review.");
                return StatusCode(500, "An error occurred while updating the review.");
            }
        }

        // Delete a Review (Only Owner Can Delete)
        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
                return NotFound("Review not found.");

            if (review.UserId != GetUserId())
                return Forbid(); // Ensure the user can only delete their own reviews

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok("Review deleted successfully.");
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .SingleOrDefault());
        }
    }
}