using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningAPI.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Security.Cryptography;
using System.Text;

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
			if (paymentRequest == null || paymentRequest.Amount <= 0 ||
				string.IsNullOrEmpty(paymentRequest.PaymentMethod) ||
				string.IsNullOrEmpty(paymentRequest.Cvc) ||
				string.IsNullOrEmpty(paymentRequest.CardNo))
			{
				return BadRequest("Invalid payment details.");
			}

	
			int userId = GetUserId();
			if (userId == 0)
			{
				return Unauthorized("User is not authenticated.");
			}


			var order = await _context.Orders
				.Where(o => o.UserId == userId && o.OrderStatus == "Pending").FirstOrDefaultAsync();

			if (order == null)
			{
				return NotFound("Order not found or is already processed.");
			}


			string salt = HashHelper.GenerateSalt();

			string hashedCardNo = HashHelper.ComputeHash(paymentRequest.CardNo, salt);
			string hashedCvc = HashHelper.ComputeHash(paymentRequest.Cvc, salt);

			var payment = new Payment
			{
				OrderId = order.OrderId,
				UserId = userId,
				PaymentMethod = paymentRequest.PaymentMethod,
				CustomerName = paymentRequest.CustomerName,
				Cvc = hashedCvc,
				CardNo = hashedCardNo,
				Amount = paymentRequest.Amount,
				PaymentDate = DateTime.UtcNow,
				Status = "Completed" 
			};

			_context.Payments.Add(payment);

			order.OrderStatus = "Paid";

			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(ProcessPayment), new { paymentId = payment.PaymentId }, payment);
		}

		public static class HashHelper
		{
			public static string ComputeHash(string input, string salt)
			{
				using (var sha256 = SHA256.Create())
				{
					var saltedInput = input + salt;
					var bytes = Encoding.UTF8.GetBytes(saltedInput);
					var hash = sha256.ComputeHash(bytes);
					return Convert.ToBase64String(hash);
				}
			}

			public static string GenerateSalt()
			{
				var randomBytes = new byte[16];
				using (var rng = RandomNumberGenerator.Create())
				{
					rng.GetBytes(randomBytes);
				}
				return Convert.ToBase64String(randomBytes);
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
