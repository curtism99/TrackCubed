using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrackCubed.Api.Data;

namespace TrackCubed.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Tags/suggest?prefix=...
        [HttpGet("suggest")]
        public async Task<ActionResult<IEnumerable<string>>> GetTagSuggestions([FromQuery] string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
            {
                return Ok(new List<string>()); // Don't search for very short strings
            }

            var lowerPrefix = prefix.ToLower();

            var suggestions = await _context.Tags
                                            .Where(t => t.Name.ToLower().StartsWith(lowerPrefix))
                                            .OrderBy(t => t.Name)
                                            .Select(t => t.Name)
                                            .Take(10) // IMPORTANT: Limit the number of results for performance
                                            .ToListAsync();

            return Ok(suggestions);
        }

        // DELETE: api/Tags/orphaned
        [HttpDelete("orphaned")]
        public async Task<IActionResult> DeleteOrphanedTags()
        {
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null)
            {
                return Unauthorized();
            }

            // 1. Find all tags that belong to the current user AND have no associated CubedItems.
            //    EF Core translates !t.CubedItems.Any() into an efficient SQL query.
            var orphanedTags = _context.Tags
                                       .Where(t => t.UserId == user.Id && !t.CubedItems.Any());

            // 2. Use ExecuteDeleteAsync for a highly efficient bulk delete operation.
            //    This runs a single, fast DELETE statement on the database.
            int deletedCount = await orphanedTags.ExecuteDeleteAsync();

            return Ok(new { DeletedCount = deletedCount });
        }
    }
}
