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

        [HttpPut("{id}"), Authorize]
        [ProducesResponseType(typeof(FeedbackDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EditFeedback(int id, UpdateFeedbackRequest request)
        {
            try
            {
                int userId = GetUserId();

                var feedback = await _context.Feedbacks.SingleOrDefaultAsync(f => f.Id == id && f.UserId == userId);
                if (feedback == null)
                {
                    return NotFound("Feedback not found or does not belong to the user.");
                }

                feedback.Rating = request.Rating;
                feedback.FeedbackContent = request.FeedbackContent?.Trim();
                feedback.UpdatedAt = DateTime.Now;

                _context.Feedbacks.Update(feedback);
                await _context.SaveChangesAsync();

                var feedbackDTO = _mapper.Map<FeedbackDTO>(feedback);
                return Ok(feedbackDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating feedback");
                return StatusCode(500, "An error occurred while updating feedback.");
            }
        }

        [HttpDelete("{id}"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            try
            {
                int userId = GetUserId();

                var feedback = await _context.Feedbacks.SingleOrDefaultAsync(f => f.Id == id && f.UserId == userId);
                if (feedback == null)
                {
                    return NotFound("Feedback not found or does not belong to the user.");
                }

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

        [HttpDelete("admin/{id}"), Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFeedbackByAdmin(int id)
        {
            try
            {
                var feedback = await _context.Feedbacks.SingleOrDefaultAsync(f => f.Id == id);
                if (feedback == null)
                {
                    return NotFound("Feedback not found.");
                }

                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();

                return Ok("Feedback deleted successfully by admin.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting feedback as admin");
                return StatusCode(500, "An error occurred while deleting feedback.");
            }
        }

        [HttpGet("my-feedbacks"), Authorize]
        [ProducesResponseType(typeof(IEnumerable<FeedbackDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyFeedbacks()
        {
            try
            {
                int userId = GetUserId();

                var feedbacks = await _context.Feedbacks
                    .Where(f => f.UserId == userId)
                    .OrderBy(f => f.UpdatedAt)
                    .ToListAsync();

                var feedbackDTOs = _mapper.Map<IEnumerable<FeedbackDTO>>(feedbacks);
                return Ok(feedbackDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving user feedbacks");
                return StatusCode(500, "An error occurred while retrieving feedbacks.");
            }
        }

        [HttpGet("all-feedbacks"), Authorize]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            try
            {
                var feedbacks = await _context.Feedbacks
                    .Include(f => f.User)
                    .OrderByDescending(f => f.UpdatedAt)
                    .ToListAsync();

                var feedbackDTOs = _mapper.Map<IEnumerable<FeedbackDTO>>(feedbacks);
                return Ok(feedbackDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all feedbacks");
                return StatusCode(500, "An error occurred while retrieving feedbacks.");
            }
        }

        [HttpPost, Authorize]
        [ProducesResponseType(typeof(FeedbackDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddFeedback(AddFeedbackRequest request)
        {
            try
            {
                int userId = GetUserId();

                _logger.LogInformation($"UserId used for feedback: {userId}");
                _logger.LogInformation($"FeedbackContent received: {request.FeedbackContent}");

                bool userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    return BadRequest("Invalid UserId. The user does not exist.");
                }

                var now = DateTime.Now;
                var feedback = new Feedback
                {
                    Rating = request.Rating,
                    FeedbackContent = request.FeedbackContent?.Trim(),
                    CreatedAt = now,
                    UpdatedAt = now,
                    UserId = userId
                };

                await _context.Feedbacks.AddAsync(feedback);
                await _context.SaveChangesAsync();

                var feedbackDTO = _mapper.Map<FeedbackDTO>(feedback);
                return Ok(feedbackDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding feedback");
                return StatusCode(500, "An error occurred while saving feedback.");
            }
        }

        private int GetUserId()
        {
            return Convert.ToInt32(User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
