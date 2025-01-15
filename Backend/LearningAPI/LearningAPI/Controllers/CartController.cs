﻿using AutoMapper;
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
	[Route("cart")]
	public class CartController : ControllerBase
	{
		private readonly MyDbContext _context;

		public CartController(MyDbContext context)
		{
			_context = context;
		}

		// POST: Add item to Cart
		[HttpPost, Authorize]
		public async Task<ActionResult<IEnumerable<Cart>>> AddToCart(List<Cart> cartItems)
		{
			if (cartItems == null || !cartItems.Any() || cartItems.Any(c => c.ProductId <= 0 || c.Quantity <= 0))
			{
				return BadRequest("Invalid cart details.");
			}

			// Get the current user's ID from the claims
			int userId = GetUserId();
			if (userId == 0)
			{
				return Unauthorized("User is not authenticated.");
			}

			foreach (var cart in cartItems)
			{
				// Set the user ID to the current user's ID for each item
				cart.UserId = userId;

				// Check if the item already exists in the user's cart
				var existingCartItem = await _context.Carts
					.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == cart.ProductId && c.Size == cart.Size);

				if (existingCartItem != null)
				{
					// If the item exists, update the quantity
					existingCartItem.Quantity += cart.Quantity;
				}
				else
				{
					// If the item does not exist, add a new entry
					_context.Carts.Add(cart);
				}
			}

			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetCartByUserId), new { userId = userId }, cartItems);
		}



		// GET: View Cart by User ID
		[HttpGet("{userId}")]
		public async Task<ActionResult<IEnumerable<Cart>>> GetCartByUserId(int userId)
		{
			var cart = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();

			if (!cart.Any())
			{
				return NotFound("Cart is empty.");
			}

			return Ok(cart);
		}

		// GET: View Cart by CartId
		[HttpGet("cartitem/{cartId}")]
		public async Task<ActionResult<Cart>> GetCartByCartId(int cartId)
		{
			var cart = await _context.Carts.FindAsync(cartId);

			if (cart == null)
			{
				return NotFound("Cart item not found.");
			}

			return Ok(cart);
		}




		[HttpPut("{cartId}"), Authorize]
		public async Task<IActionResult> UpdateCartItem(int cartId, [FromBody] UpdateCartItemRequest request)
		{
			var cartItem = await _context.Carts.FindAsync(cartId);

			if (cartItem == null)
			{
				return NotFound("Cart item not found.");
			}

			if (request.Quantity <= 0)
			{
				return BadRequest("Quantity must be greater than 0.");
			}

			// Update the cart item
			cartItem.Quantity = request.Quantity;
			await _context.SaveChangesAsync();
			return Ok();


		}

		// DELETE: Remove Cart Item
		[HttpDelete("{cartId}")]
		public async Task<IActionResult> RemoveCartItem(int cartId)
		{
			var cartItem = await _context.Carts.FindAsync(cartId);

			if (cartItem == null)
			{
				return NotFound("Cart item not found.");
			}

			_context.Carts.Remove(cartItem);
			await _context.SaveChangesAsync();
			return NoContent();
		}

		private int GetUserId()
		{
			return Convert.ToInt32(User.Claims
				.Where(c => c.Type == ClaimTypes.NameIdentifier)
				.Select(c => c.Value).SingleOrDefault());
		}
	}



}