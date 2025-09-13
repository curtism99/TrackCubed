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
    public class ItemTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ItemTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetItemTypes()
        {
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null) return Unauthorized();

            // 1. Get the untouchable system types
            var systemTypes = await _context.SystemItemTypes.Select(t => t.Name).ToListAsync();
            // 2. Get this user's custom types
            var userTypes = await _context.UserItemTypes.Where(t => t.UserId == user.Id).Select(t => t.Name).ToListAsync();

            // 3. Combine, order, and return the list
            var allTypes = systemTypes.Concat(userTypes).OrderBy(name => name).Distinct().ToList();

            return Ok(allTypes);
        }
    }
}
