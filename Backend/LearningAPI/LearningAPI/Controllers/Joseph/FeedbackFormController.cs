using AutoMapper;
using System.Security.Claims;
using LearningAPI.Models.Joseph;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningAPI.Controllers.Joseph
{
    [ApiController]
    [Route("[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(MyDbContext context, IMapper mapper, ILogger<FeedbackController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpDelete("{id}"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            try
            {
                // Get the logged-in user's ID
                int userId = GetUserId();
                bool isAdmin = User.IsInRole("Admin");

                // Find the feedback
                var feedback = await _context.Feedbacks.SingleOrDefaultAsync(f => f.Id == id);

                // Check if feedback exists
                if (feedback == null)
                {
                    return NotFound("Feedback not found.");
                }

                // Allow admins to delete any feedback, but regular users can only delete their own
                if (!isAdmin && feedback.UserId != userId)
                {
                    return Forbid("You do not have permission to delete this feedback.");
                }

                // Delete the feedback
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();

                return Ok("Feedback deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting feedback");
                return StatusCode(500, "An error occurred while deleting feedback.");
            }
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
        }
    }
}