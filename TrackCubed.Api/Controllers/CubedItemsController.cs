using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrackCubed.Api.Data;
using TrackCubed.Shared.Models;

namespace TrackCubed.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All actions in this controller require the user to be logged in
    public class CubedItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CubedItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CubedItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CubedItem>>> GetMyCubedItems()
        {
            // 1. Get the unique ID of the user from the access token.
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(entraObjectId))
            {
                return Unauthorized(); // Should not happen if [Authorize] is working
            }

            // 2. Find the user's internal ID in your database.
            var user = await _context.ApplicationUsers
                                     .AsNoTracking() // Read-only query for performance
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);

            if (user == null)
            {
                // This could happen if the onboard process failed for some reason.
                return NotFound("User profile not found.");
            }

            // 3. Fetch all CubedItems belonging to that user.
            //    Order by the most recently created.
            var items = await _context.CubedItems
                                      .Where(c => c.CreatedById == user.Id)
                                      .OrderByDescending(c => c.CreatedOn)
                                      .ToListAsync();

            return Ok(items);
        }

        // We will add [HttpPost] for creating items later.
    }
}
