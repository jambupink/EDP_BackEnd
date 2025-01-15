using AutoMapper;
using LearningAPI.Models;
using LearningAPI.Models.Latiff;
using Microsoft.AspNetCore.Mvc;

namespace LearningAPI.Controllers.Latiff
{
    [ApiController]
    [Route("[controller]")]
    public class UserRoleController(MyDbContext context, IMapper mapper, ILogger<UserRoleController> logger) : ControllerBase
    {
        private readonly MyDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UserRoleController> _logger = logger;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserRoleDTO>), StatusCodes.Status200OK)]
        public IActionResult GetAll(string? search)
        {
            try
            {
                IQueryable<UserRole> result = _context.UserRoles;
                if (search != null)
                {
                    {
                        result = result.Where(x => x.Role.Contains(search)
                        || x.Description.Contains(search));
                    }
                }

                var list = result.OrderByDescending(x => x.CreatedAt).ToList();
                IEnumerable<UserRoleDTO> data = list.Select(_mapper.Map<UserRoleDTO>);

                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when getting all userRoles");
                return StatusCode(500);
            }
            
        }

        [HttpPost]
        public IActionResult AddUserRole(UserRole userRole)
        {
            try
            {
                var now = DateTime.Now;
                var myUserRole = new UserRole()
                {
                    Role = userRole.Role.Trim(),
                    Description = userRole.Description.Trim(),
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.UserRoles.Add(myUserRole);
                _context.SaveChanges();
                return Ok(myUserRole);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when adding role");
                return StatusCode(500);
            }

        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IEnumerable<UserRoleDTO>), StatusCodes.Status200OK)]
        public IActionResult GetUserRole(int id)
        {
            try
            {
                UserRole? userRole = _context.UserRoles.Find(id);

                if (userRole == null)
                {
                    return NotFound();
                }
                return Ok(userRole);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when getting userRoles by id");
                return StatusCode(500);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUserRole(int id, UserRole userRole)
        {
            try
            {
                var myUserRole = _context.UserRoles.Find(id);
                if (myUserRole == null)
                {
                    return NotFound();
                }

                myUserRole.Role = userRole.Role.Trim();
                myUserRole.Description = userRole.Description.Trim();
                myUserRole.UpdatedAt = DateTime.Now;

                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when updating userRoles by id");
                return StatusCode(500);
            }

        }
        [HttpDelete("{id}")]
        public IActionResult DeleteUserRole(int id)
        {
            try
            {
                var myUserRole = _context.UserRoles.Find(id);
                if (myUserRole == null)
                {
                    return NotFound();
                }
                _context.UserRoles.Remove(myUserRole);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when deleting userRoles by id");
                return StatusCode(500);
            }
        }
    }
}
