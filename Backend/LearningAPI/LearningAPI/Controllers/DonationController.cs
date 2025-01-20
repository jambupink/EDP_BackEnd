using AutoMapper;
using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace LearningAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class DonationController(MyDbContext context, IMapper mapper,
		ILogger<DonationController> logger) : ControllerBase
	{

		[HttpPost("/donate"), Authorize]
		[ProducesResponseType(typeof(DonationDTO), StatusCodes.Status200OK)]
		public async Task<IActionResult> makeDonation(DonationRequest donationRequest)
		{
			try
			{

				int userId = GetUserId();
				var now = DateTime.Now;
				var format = "yyyy-MM-dd HH:mm:ss";
				DateTime? dateTime = null;

				if (donationRequest.DonationDateTime != null)
				{
					dateTime = DateTime.ParseExact(donationRequest.DonationDateTime, format, CultureInfo.InvariantCulture);
				}

				var donation = new Donation()
				{
					UserId = userId,
					CreatedAt = now,
					UpdatedAt = now,

					Title = donationRequest.Title,
					Description = donationRequest.Description,
					ImageFile = donationRequest.ImageFile,
					DonationDateTime = dateTime,
					Condition = donationRequest.Condition
				};


				await context.Donations.AddAsync(donation);
				await context.SaveChangesAsync();

				DonationDTO donationDTO = mapper.Map<DonationDTO>(donation);

				return Ok(donationDTO);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error in updating donation");
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
