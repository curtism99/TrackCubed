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
    [Authorize]
    public class ItemTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ItemTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemType>>> GetItemTypes() // <-- Returns the full object
        {
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null) return Unauthorized();

            var itemTypes = await _context.ItemTypes
                                           .Where(it => it.UserId == null || it.UserId == user.Id)
                                           .OrderBy(t => t.Name)
                                           .AsNoTracking()
                                           .ToListAsync();

            return Ok(itemTypes);
        }


        // DELETE: api/ItemTypes/orphaned-custom
        [HttpDelete("orphaned-custom")]
        public async Task<IActionResult> DeleteOrphanedUserItemTypes()
        {
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null)
            {
                return Unauthorized();
            }

            // Find all custom item types for this user that are NOT referenced in the CubedItems table.
            // This is the simple, performant query that the foreign key makes possible.
            var orphanedTypesQuery = _context.ItemTypes
                .Where(it => it.UserId == user.Id &&
                             !_context.CubedItems.Any(ci => ci.ItemTypeId == it.Id));

            // Use ExecuteDeleteAsync to perform a single, fast DELETE command on the database.
            int deletedCount = await orphanedTypesQuery.ExecuteDeleteAsync();

            return Ok(new { DeletedCount = deletedCount });
        }
    }
}
