using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrackCubed.Api.Data;
using TrackCubed.Shared.DTOs;
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
        public async Task<ActionResult<IEnumerable<CubedItemDto>>> GetMyCubedItems()
        {
            // 1. Get the unique ID of the user from the access token (no change here).
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(entraObjectId))
            {
                return Unauthorized();
            }

            // 2. Find the user's internal ID in your database (no change here).
            var user = await _context.ApplicationUsers
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);

            if (user == null)
            {
                return NotFound("User profile not found.");
            }

            // 3. Fetch and map the CubedItems belonging to that user in a single, efficient query.
            var items = await _context.CubedItems
                                      .Where(c => c.CreatedById == user.Id) // Filter by user
                                      .OrderByDescending(c => c.CreatedOn) // Sort by newest
                                      .Select(c => new CubedItemDto // <-- THIS IS THE KEY CHANGE
                                      {
                                          // Map the entity properties to the DTO properties
                                          Id = c.Id,
                                          Name = c.Name,
                                          Link = c.Link,
                                          Description = c.Description,
                                          ItemType = c.ItemType,
                                          CreatedOn = c.CreatedOn,
                                          CreatedById = c.CreatedById
                                      })
                                      .ToListAsync(); // Execute the optimized query

            return Ok(items);
        }


        // POST: api/CubedItems
        [HttpPost]
        public async Task<ActionResult<CubedItem>> CreateCubedItem(CubedItemCreateDto itemDto)
        {
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null)
            {
                return Unauthorized("User profile not found.");
            }

            // Manually map from the DTO to the full CubedItem entity
            var newCubedItem = new CubedItem
            {
                // Properties from the DTO
                Name = itemDto.Name,
                Link = itemDto.Link,
                Description = itemDto.Description,
                Notes = itemDto.Notes,
                ItemType = itemDto.ItemType,

                // Properties set by the server (cannot be set by the client)
                Id = Guid.NewGuid(),
                CreatedById = user.Id,
                CreatedOn = DateTime.UtcNow,
                DateLastAccessed = DateTime.UtcNow
            };

            _context.CubedItems.Add(newCubedItem);
            await _context.SaveChangesAsync();

            // Map the final entity to a safe DTO for the response.
            var createdItemDto = new CubedItemDto
            {
                Id = newCubedItem.Id,
                Name = newCubedItem.Name,
                Link = newCubedItem.Link,
                Description = newCubedItem.Description,
                ItemType = newCubedItem.ItemType,
                CreatedOn = newCubedItem.CreatedOn,
                CreatedById = newCubedItem.CreatedById
            };

            // Return the DTO
            return CreatedAtAction(nameof(GetMyCubedItems), new { id = createdItemDto.Id }, createdItemDto);
        }
    }
}
