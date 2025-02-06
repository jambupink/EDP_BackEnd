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
	public class DonationController : ControllerBase
	{
		private readonly MyDbContext context;
		private readonly IMapper mapper;
		private readonly ILogger<DonationController> logger;

		public DonationController(MyDbContext context, IMapper mapper, ILogger<DonationController> logger)
		{
			this.context = context;
			this.mapper = mapper;
			this.logger = logger;
		}

		//[HttpPost("/donate"), Authorize]
		//[ProducesResponseType(typeof(DonationDTO), StatusCodes.Status200OK)]
		//public async Task<IActionResult> makeDonation(DonationRequest donationRequest)
		//{
		//	try
		//	{
		//		int userId = GetUserId();
		//		var now = DateTime.Now;
		//		var format = "yyyy-MM-dd HH:mm:ss";
		//		DateTime? dateTime = null;
		//
		//		if (donationRequest.DonationDateTime != null)
		//		{
		//			dateTime = DateTime.ParseExact(donationRequest.DonationDateTime, format, CultureInfo.InvariantCulture);
		//		}

		//		var donation = new Donation()
		//		{
		//			UserId = userId,
		//			CreatedAt = now,
		//			UpdatedAt = now,
		//			Title = donationRequest.Title,
		//			Description = donationRequest.Description,
		//			ImageFile = donationRequest.ImageFile,
		//			DonationDateTime = dateTime,
		//			Condition = donationRequest.Condition
		//		};

		//		await context.Donations.AddAsync(donation);
		//		await context.SaveChangesAsync();

		//		DonationDTO donationDTO = mapper.Map<DonationDTO>(donation);

		//		return Ok(donationDTO);
		//	}
		//	catch (Exception ex)
		//	{
		//		logger.LogError(ex, "Error in updating donation");
		//		return StatusCode(500);
		//	}
		//}

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

        // Assign default status "Pending" to the new donation
        var donationStatus = new DonationStatus()
        {
            DonationId = donation.Id,
            Status = "Pending",
            UpdatedAt = DateTime.UtcNow
        };

        await context.DonationStatuses.AddAsync(donationStatus);
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




		[HttpGet("latest"), Authorize]
		[ProducesResponseType(typeof(DonationDTO), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetLatestDonation()
		{
			try
			{
				int userId = GetUserId();

				var latestDonation = await context.Donations
					.Where(d => d.UserId == userId)
					.OrderByDescending(d => d.CreatedAt)
					.FirstOrDefaultAsync();

				if (latestDonation == null)
				{
					return NotFound(new { message = "No donations found." });
				}

				DonationDTO donationDTO = mapper.Map<DonationDTO>(latestDonation);
				return Ok(donationDTO);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error fetching the latest donation");
				return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
			}
		}

		//	[HttpPut("{id}/datetime"), Authorize]
		//	[ProducesResponseType(StatusCodes.Status200OK)]
		//	[ProducesResponseType(StatusCodes.Status404NotFound)]
		//	public async Task<IActionResult> UpdateDonationDateTime(int id, [FromBody] UpdateDateTimeRequest request)
		//	{
		//		try
		//		{
		//			var donation = await context.Donations.FirstOrDefaultAsync(d => d.Id == id);
		//
		//			if (donation == null)
		//			{
		//				return NotFound(new { message = "Donation not found." });
		//			}
		//
		//			if (donation.UserId != GetUserId())
		//			{
		//				return Unauthorized(new { message = "You are not authorized to update this donation." });
		//			}
		//
		//			donation.DonationDateTime = DateTime.ParseExact(request.DonationDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		//			donation.UpdatedAt = DateTime.UtcNow;

		//			await context.SaveChangesAsync();

		//			return Ok(new { message = "Date & Time updated successfully." });
		//		}
		//		catch (Exception ex)
		//		{
		//			logger.LogError(ex, "Error updating donation date & time");
		//			return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
		//		}
		//	}

		[HttpPut("{id}/datetime"), Authorize]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> UpdateDonationDateTime(int id, [FromBody] UpdateDateTimeRequest request)
		{
			try
			{
				var donation = await context.Donations.FirstOrDefaultAsync(d => d.Id == id);

				if (donation == null)
				{
					return NotFound(new { message = "Donation not found." });
				}

				if (donation.UserId != GetUserId())
				{
					return Unauthorized(new { message = "You are not authorized to update this donation." });
				}

				// Convert new date
				DateTime newDateTime = DateTime.ParseExact(request.DonationDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
				string changeLog = $"Pickup Date changed from {donation.DonationDateTime} to {newDateTime}";

				// Update donation
				donation.DonationDateTime = newDateTime;
				donation.UpdatedAt = DateTime.UtcNow;

				// Save changes
				await context.SaveChangesAsync();

				// Log the change in DonationHistory
				var historyRecord = new DonationHistory
				{
					DonationId = id,
					ChangeDescription = changeLog,
					ChangeDate = DateTime.UtcNow
				};

				await context.DonationHistories.AddAsync(historyRecord);
				await context.SaveChangesAsync();

				return Ok(new { message = "Date & Time updated successfully." });
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error updating donation date & time");
				return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
			}
		}


		[HttpGet("user-donations"), Authorize]
		[ProducesResponseType(typeof(List<DonationDTO>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetUserDonations()
		{
			try
			{
				int userId = GetUserId();

				// Retrieve all donations for the authenticated user
				var userDonations = await context.Donations
					.Where(d => d.UserId == userId)
					.OrderByDescending(d => d.CreatedAt) // Optional: Order by latest donations
					.ToListAsync();

				if (userDonations == null || !userDonations.Any())
				{
					return NotFound(new { message = "No donations found." });
				}

				// Map to DTOs for response
				var donationDTOs = mapper.Map<List<DonationDTO>>(userDonations);

				return Ok(donationDTOs);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error fetching user donations");
				return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
			}
		}


		[HttpDelete("{id}/image"), Authorize]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteDonationImage(int id)
		{
			try
			{
				var donation = await context.Donations.FirstOrDefaultAsync(d => d.Id == id);

				if (donation == null)
				{
					return NotFound(new { message = "Donation not found." });
				}

				if (donation.UserId != GetUserId())
				{
					return Unauthorized(new { message = "You are not authorized to delete this image." });
				}

				if (string.IsNullOrEmpty(donation.ImageFile))
				{
					return BadRequest(new { message = "No image to delete." });
				}

				// Delete the image file from the server or storage
				var imagePath = Path.Combine("wwwroot", "uploads", donation.ImageFile);
				if (System.IO.File.Exists(imagePath))
				{
					System.IO.File.Delete(imagePath);
				}

				// Remove the image reference from the database
				donation.ImageFile = null;
				donation.UpdatedAt = DateTime.UtcNow;

				await context.SaveChangesAsync();

				return Ok(new { message = "Image deleted successfully." });
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error deleting donation image");
				return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
			}
		}


		[HttpGet("{donationId}/history"), Authorize]
		[ProducesResponseType(typeof(List<DonationHistory>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetDonationHistory(int donationId)
		{
			try
			{
				int userId = GetUserId();

				// Check if the donation belongs to the user
				var donation = await context.Donations
					.Where(d => d.Id == donationId && d.UserId == userId)
					.FirstOrDefaultAsync();

				if (donation == null)
				{
					return NotFound(new { message = "Donation not found or not authorized." });
				}

				// Get the history
				var history = await context.DonationHistories
					.Where(dh => dh.DonationId == donationId)
					.OrderByDescending(dh => dh.ChangeDate)
					.ToListAsync();

				if (history.Count == 0)
					return NotFound(new { message = "No history found for this donation." });

				return Ok(history);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error fetching donation history");
				return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
			}
		}

		[HttpGet("{donationId}/status"), Authorize]
		[ProducesResponseType(typeof(DonationStatus), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetDonationStatus(int donationId)
		{
			try
			{
				var status = await context.DonationStatuses
					.FirstOrDefaultAsync(ds => ds.DonationId == donationId);

				if (status == null)
				{
					return NotFound(new { message = "Status not found for this donation." });
				}

				return Ok(status);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error fetching donation status");
				return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
			}
		}

		[HttpPut("{donationId}/status"), Authorize]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> UpdateDonationStatus(int donationId, [FromBody] UpdateStatusRequest request)
		{
			try
			{
				var donationStatus = await context.DonationStatuses
					.FirstOrDefaultAsync(ds => ds.DonationId == donationId);

				if (donationStatus == null)
				{
					return NotFound(new { message = "Donation status not found." });
				}

				donationStatus.Status = request.NewStatus;
				donationStatus.UpdatedAt = DateTime.UtcNow;

				await context.SaveChangesAsync();

				return Ok(new { message = "Donation status updated successfully." });
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error updating donation status");
				return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
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


