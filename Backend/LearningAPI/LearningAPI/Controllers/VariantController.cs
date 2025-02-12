using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningAPI.Models;

namespace LearningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VariantController : ControllerBase
    {
        private readonly MyDbContext _context;

        public VariantController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Variant>>> GetVariants()
        {
            return await _context.Variants.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Variant>> GetVariant(int id)
        {
            var variant = await _context.Variants.FindAsync(id);
            if (variant == null) return NotFound();
            return variant;
        }

        [HttpPost]
        public async Task<ActionResult<Variant>> CreateVariant(Variant variant)
        {
            _context.Variants.Add(variant);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVariant), new { id = variant.VariantId }, variant);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVariant(int id)
        {
            var variant = await _context.Variants.FindAsync(id);
            if (variant == null) return NotFound();
            _context.Variants.Remove(variant);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}