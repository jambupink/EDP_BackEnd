using AutoMapper;
using LearningAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LearningAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class OrderItemController : ControllerBase
	{
		private readonly MyDbContext _context;

		public OrderItemController(MyDbContext context)
		{
			_context = context;
		}

		// POST: Add Order Item
		[HttpPost]
		public async Task<ActionResult<OrderItem>> AddOrderItem(OrderItem orderItem)
		{
			if (orderItem == null || orderItem.OrderId <= 0 || orderItem.ProductId <= 0 || orderItem.Quantity <= 0 || orderItem.Price <= 0)
			{
				return BadRequest("Invalid order item details.");
			}

			_context.OrderItems.Add(orderItem);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetOrderItemsByOrderId), new { orderId = orderItem.OrderId }, orderItem);
		}

		// GET: Get Order Items by Order ID
		[HttpGet("{orderId}")]
		public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItemsByOrderId(int orderId)
		{
			var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();

			if (!orderItems.Any())
			{
				return NotFound("No order items found for this order.");
			}

			return Ok(orderItems);
		}
	}
}
