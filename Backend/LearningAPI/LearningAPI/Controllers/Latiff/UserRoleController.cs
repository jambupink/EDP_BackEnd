using LearningAPI.Models.Latiff;
using Microsoft.AspNetCore.Mvc;

namespace LearningAPI.Controllers.Latiff
{
    [ApiController]
    [Route("[controller]")]
    public class UserRoleController(MyDbContext context) : ControllerBase
    {
        private readonly MyDbContext _context = context;

        [HttpGet]
        public IActionResult GetAll(string? search)
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
            return Ok(list);
        }

        [HttpPost]
        public IActionResult AddUserRole(UserRole userRole)
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

        [HttpGet("{id}")]
        public IActionResult GetUserRole(int id)
        {
            UserRole? userRole = _context.UserRoles.Find(id);

            if (userRole == null)
            {
                return NotFound();
            }
            return Ok(userRole);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUserRole(int id, UserRole userRole)
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
        [HttpDelete("{id}")]
        public IActionResult DeleteUserRole(int id)
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
    }
}
