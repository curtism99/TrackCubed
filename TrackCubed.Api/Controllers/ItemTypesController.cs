using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var itemTypeNames = await _context.ItemTypes
                                              .OrderBy(t => t.Name)
                                              .Select(t => t.Name)
                                              .ToListAsync();

            return Ok(itemTypeNames);
        }
    }
}
