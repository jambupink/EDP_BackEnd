﻿using AutoMapper;
using BCrypt.Net;
using LearningAPI.Models.Latiff;
using LearningAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mysqlx.Crud;
using NanoidDotNet;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LearningAPI.Controllers.Latiff
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(MyDbContext context, IConfiguration configuration, IMapper mapper,
        ILogger<UserController> logger, EmailService emailService) : ControllerBase
    {
        private readonly EmailService _emailService = emailService;

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);

            if (user == null || user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired token" });
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = String.Empty;
            user.EmailConfirmationTokenExpiry = null;

            await context.SaveChangesAsync();
            return Ok(new { message = "Email confirmed successfully" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                // Trim string values
                request.Name = request.Name.Trim();
                request.Email = request.Email.Trim().ToLower();
                request.Password = request.Password.Trim();

                // Check email
                var foundUser = await context.Users.Where(x => x.Email == request.Email).FirstOrDefaultAsync();
                if (foundUser != null)
                {
                    string message = "Email already exists.";
                    return BadRequest(new { message });
                }

                // Create user object
                var defaultRole = context.UserRoles.FirstOrDefault(ur => ur.Id == 1);
                if (defaultRole == null)
                {
                    return BadRequest(new { message = "Default user role does not exist." });
                }
                var now = DateTime.Now;
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var confirmationToken = Nanoid.Generate(size: 64);
                var user = new User()
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = passwordHash,
                    UserRoleId = 1,
                    EmailConfirmationToken = confirmationToken,
                    EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24),
                    CreatedAt = now,
                    UpdatedAt = now
                };

                var clientBaseUrl = configuration.GetValue<string>("ClientBaseUrl");
                var confirmationLink = $"{clientBaseUrl}/confirm-email?token={confirmationToken}";
                _emailService.SendConfirmationEmail(user.Email, confirmationLink);
                // Add user
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when user register");
                return StatusCode(500);
            }
        }


        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                // Trim string values
                request.Email = request.Email.Trim().ToLower();
                request.Password = request.Password.Trim();

                // Check email and password
                string message = "Email or password is not correct.";
                var foundUser = await context.Users.Where(x => x.Email == request.Email).FirstOrDefaultAsync();
                if (foundUser == null)
                {
                    return BadRequest(new { message });
                }
                bool verified = BCrypt.Net.BCrypt.Verify(request.Password, foundUser.Password);
                if (!verified)
                {
                    return BadRequest(new { message });
                }
                if (!foundUser.IsEmailConfirmed)
                {
                    return BadRequest(new { message = "Email is not verified. Please check your inbox" });
                }

                // Return user info
                UserDTO userDTO = mapper.Map<UserDTO>(foundUser);
                string accessToken = CreateToken(foundUser);
                LoginResponse response = new() { User = userDTO, AccessToken = accessToken };
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when user login");
                return StatusCode(500);
            }
        }

        [HttpGet("auth"), Authorize]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        public IActionResult Auth()
        {
            try
            {
                var id = Convert.ToInt32(User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value).SingleOrDefault());
                var name = User.Claims.Where(c => c.Type == ClaimTypes.Name)
                    .Select(c => c.Value).SingleOrDefault();
                var email = User.Claims.Where(c => c.Type == ClaimTypes.Email)
                    .Select(c => c.Value).SingleOrDefault();
                var userRoleId = Convert.ToInt32(User.Claims.Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value).SingleOrDefault()); 

                if (id != 0 && name != null && email != null && userRoleId != 0)
                {
                    UserDTO userDTO = new() { Id = id, Name = name, Email = email, UserRoleId = userRoleId };
                    AuthResponse response = new() { User = userDTO };
                    return Ok(response);
                }
                else
                {
                    return Unauthorized(userRoleId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when user auth");
                return StatusCode(500);
            }
        }

        private string CreateToken(User user)
        {
            string? secret = configuration.GetValue<string>("Authentication:Secret");
            if (string.IsNullOrEmpty(secret))
            {
                throw new Exception("Secret is required for JWT authentication.");
            }

            int tokenExpiresDays = configuration.GetValue<int>("Authentication:TokenExpiresDays");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.UserRoleId.ToString())
                ]),
                Expires = DateTime.UtcNow.AddDays(tokenExpiresDays),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(securityToken);

            return token;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(string? search)
        {
            try
            {
                IQueryable<User> query = context.Users;

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
                }

                //retrieve all users
                var users = await query.ToListAsync();
                var userDTOs = users.Select(mapper.Map<UserDTO>);

                return Ok(userDTOs);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when fetching all users");

                return StatusCode(500);
            }
        }

        [HttpGet("{id}"), Authorize]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await context.Users.SingleOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var (userId, userRoleId) = GetUserInfo();

                // ✅ Allow user to view their own profile OR admins to view any profile
                if (user.Id != userId && userRoleId != 2)
                {
                    return Forbid();
                }
                var UserDTO = mapper.Map<UserDTO>(user);

                return Ok(UserDTO);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when fetching user by ID");

                return StatusCode(500);
            }
        }
        [HttpPut("password/{id}"), Authorize]
        public async Task<IActionResult> UpdateUserPassword(int id, UpdateUserPasswordRequest user)
        {
            try
            {
                logger.LogInformation($"Received request to update password for user ID: {id}");
                //Find user
                var myUser = await context.Users.FindAsync(id);
                if (myUser == null)
                {
                    return NotFound();
                }

                var (userId, userRoleId) = GetUserInfo();

                if (myUser.Id != userId && userRoleId != 2)
                {
                    return Forbid();
                }

                if (user.Password != null)
                {
                    string message = "Email or password is not correct.";
                    bool verified = BCrypt.Net.BCrypt.Verify(user.Password, myUser.Password);
                    if (!verified)
                    {
                        return BadRequest(new { message });
                    }
                }

                
                if (user.NewPassword != null)
                {
                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.NewPassword);
                    myUser.Password = passwordHash;
                }
                
                myUser.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when updating user password");
                return StatusCode(500);
            }
        }

        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest user)
        {
            try
            {
                var myUser = await context.Users.FindAsync(id);
                if (myUser == null)
                {
                    return NotFound();
                }

                var (userId, userRoleId) = GetUserInfo();

                if (myUser.Id != userId && userRoleId != 2)
                {
                    return Forbid();
                }

                if (user.Name != null)
                {
                    myUser.Name = user.Name.Trim();
                }
                if (user.Email != null)
                {
                    myUser.Email = user.Email.Trim();
                }
                if (user.Password != null)
                {
                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    myUser.Password = passwordHash;
                }
                if (user.Gender != null)
                {
                    myUser.Gender = user.Gender.Trim();
                }
                if (user.MobileNumber != null)
                {
                    myUser.MobileNumber = user.MobileNumber.Trim();
                }
                if (user.Address != null)
                {
                    myUser.Address = user.Address.Trim();
                }
                if (user.Points != null)
                {
                    myUser.Points = (int)user.Points;
                }
                if (user.UserRoleId != null)
                {
                    myUser.UserRoleId = (int)user.UserRoleId;
                }
                if (user.IsEmailConfirmed != null)
                {
                    myUser.IsEmailConfirmed = (bool)user.IsEmailConfirmed;
                }
                myUser.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when updating user");
                return StatusCode(500);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return Ok(); // Don't reveal email doesn't exist

            // Generate token (reuse email confirmation fields)
            user.EmailConfirmationToken = Nanoid.Generate(size: 64);
            user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(1);
            user.UpdatedAt = DateTime.Now;

            await context.SaveChangesAsync();

            // Send email
            var clientBaseUrl = configuration.GetValue<string>("ClientBaseUrl");
            var resetLink = $"{clientBaseUrl}/reset-password?token={user.EmailConfirmationToken}";
            _emailService.SendPasswordResetEmail(user.Email, resetLink);

            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u =>
                u.EmailConfirmationToken == request.Token &&
                u.EmailConfirmationTokenExpiry > DateTime.UtcNow);

            if (user == null) return BadRequest("Invalid or expired token");

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.EmailConfirmationToken = String.Empty;
            user.EmailConfirmationTokenExpiry = null;

            await context.SaveChangesAsync();
            return Ok(new { message =  "Password reset successfully" });
        }

        private (int userId, int userRoleId) GetUserInfo()
        {
            int userId = Convert.ToInt32(User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .SingleOrDefault());

            int userRoleId = Convert.ToInt32(User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .SingleOrDefault());

            return (userId, userRoleId);
        }

        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var myUser = context.Users.Find(id);
                if (myUser == null)
                {
                    return NotFound();
                }

                var (userId, userRoleId) = GetUserInfo();

                if (myUser.Id != userId && userRoleId != 2)
                {
                    return Forbid();
                }

                context.Users.Remove(myUser);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when deleting User");
                return StatusCode(500);
            }
        }
    }
}