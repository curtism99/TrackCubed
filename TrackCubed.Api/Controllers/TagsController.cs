using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    }
}
