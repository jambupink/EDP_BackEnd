using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningAPI.Models;
using System.Threading.Tasks;
using System.Security.Claims;

namespace LearningAPI.Controllers
{
	[ApiController]
	[Route("payment")]
	public class PaymentController : ControllerBase
	{
		private readonly MyDbContext _context;

		public PaymentController(MyDbContext context)
		{
			_context = context;
		}

		// POST: Process payment for an order
		[HttpPost, Authorize]
		public async Task<ActionResult<Payment>> ProcessPayment([FromBody] PaymentRequest paymentRequest)
		{
			if (paymentRequest == null || paymentRequest.Amount <= 0 || string.IsNullOrEmpty(paymentRequest.PaymentMethod) || string.IsNullOrEmpty(paymentRequest.Cvc))
			{
				return BadRequest("Invalid payment details.");
			}

			// Get the current user's ID from the claims
			int userId = GetUserId();
			if (userId == 0)
			{
				return Unauthorized("User is not authenticated.");
			}

			// Validate the OrderId passed in the payment request
			var order = await _context.Orders
				.Where(o => o.OrderId == paymentRequest.OrderId && o.UserId == userId && o.OrderStatus == "Pending")
				.FirstOrDefaultAsync();

			if (order == null)
			{
				return NotFound("Order not found or is already processed.");
			}

			// Create the payment record
			var payment = new Payment
			{
				OrderId = order.OrderId,
				UserId = userId,
				PaymentMethod = paymentRequest.PaymentMethod,
				CustomerName = paymentRequest.CustomerName,
				Cvc = paymentRequest.Cvc,
				Amount = paymentRequest.Amount,
				PaymentDate = DateTime.UtcNow,
				Status = "Completed" // Assuming the payment is successful
			};

			_context.Payments.Add(payment);

			// Update the order status to "Paid"
			order.OrderStatus = "Paid";

			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(ProcessPayment), new { paymentId = payment.PaymentId }, payment);
		}

		// Helper method to get the userId from claims
		private int GetUserId()
		{
			return Convert.ToInt32(User.Claims
				.Where(c => c.Type == ClaimTypes.NameIdentifier)
				.Select(c => c.Value).SingleOrDefault());
		}
	}


}
