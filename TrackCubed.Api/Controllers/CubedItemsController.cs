using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
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
                                          Notes = c.Notes,
                                          CreatedOn = c.CreatedOn,
                                          CreatedById = c.CreatedById,
                                          Tags = c.Tags.Select(t => t.Name).ToList()
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

            await UpdateItemTags(newCubedItem, itemDto.Tags);

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


        // DELETE: api/CubedItems/1234-5678-9012-3456
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCubedItem(Guid id)
        {
            // 1. Get the current user from the token.
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null)
            {
                return Unauthorized();
            }

            // 2. Find the item to delete.
            // CRITICAL SECURITY CHECK: We must ensure the item exists AND belongs to the current user.
            // This prevents a user from deleting another user's items by guessing a Guid.
            var itemToDelete = await _context.CubedItems
                                             .FirstOrDefaultAsync(c => c.Id == id && c.CreatedById == user.Id);

            // 3. If the item doesn't exist or doesn't belong to the user, return NotFound.
            if (itemToDelete == null)
            {
                return NotFound("Item not found or you do not have permission to delete it.");
            }

            // 4. Remove the item and save changes.
            _context.CubedItems.Remove(itemToDelete);
            await _context.SaveChangesAsync();

            // 5. Return a "204 No Content" response, which is the standard for a successful DELETE.
            return NoContent();
        }

        // PUT: api/CubedItems/1234-5678-9012-3456
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCubedItem(Guid id, CubedItemDto itemDto)
        {
            // A quick check to ensure the ID in the URL matches the ID in the body, if it exists.
            if (id != itemDto.Id)
            {
                return BadRequest("ID mismatch.");
            }

            // 1. Get the current user.
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null)
            {
                return Unauthorized();
            }

            // 2. Find the existing item in the database.
            // CRITICAL SECURITY CHECK: Ensure the item belongs to the current user.
            var itemToUpdate = await _context.CubedItems
                                             .FirstOrDefaultAsync(c => c.Id == id && c.CreatedById == user.Id);

            if (itemToUpdate == null)
            {
                return NotFound("Item not found or you do not have permission to edit it.");
            }

            // 3. Update properties from the DTO
            itemToUpdate.Name = itemDto.Name;
            itemToUpdate.Link = itemDto.Link;
            itemToUpdate.Description = itemDto.Description;
            itemToUpdate.Notes = itemDto.Notes;
            itemToUpdate.ItemType = itemDto.ItemType;
            itemToUpdate.DateLastAccessed = DateTime.UtcNow;
            // Note: We do NOT update CreatedOn or CreatedById.


            // Make sure to include the existing tags so EF Core can track changes
            await _context.Entry(itemToUpdate).Collection(i => i.Tags).LoadAsync();
            await UpdateItemTags(itemToUpdate, itemDto.Tags);

            // 4. Save the changes to the database.
            await _context.SaveChangesAsync();

            // 5. Return "204 No Content" for a successful update.
            return NoContent();
        }

        private async Task UpdateItemTags(CubedItem item, List<string> tagNames)
        {
            // Clear existing tags to handle removals
            item.Tags.Clear();

            if (tagNames == null || !tagNames.Any()) return;

            foreach (var tagName in tagNames.Select(t => t.Trim().ToLower()).Distinct())
            {
                if (string.IsNullOrWhiteSpace(tagName)) continue;

                // Find an existing tag or create a new one
                var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                if (existingTag != null)
                {
                    item.Tags.Add(existingTag);
                }
                else
                {
                    item.Tags.Add(new Tag { Name = tagName });
                }
            }
        }

        // GET: api/CubedItems/search?searchText=...&itemType=...&tags=...
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CubedItemDto>>> SearchMyCubedItems(
            [FromQuery] string? searchText,
            [FromQuery] string? itemType,
            [FromQuery] List<string>? tags,
            [FromQuery] string tagMode = "any") // "any" for inclusive, "all" for exclusive
        {
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null)
            {
                return Unauthorized();
            }

            // 1. Start with a base query for the current user's items.
            // IQueryable is essential here - it lets us build the query step-by-step.
            var query = _context.CubedItems.Where(c => c.CreatedById == user.Id);

            // 2. Add full-text search filter (if provided)
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var lowerSearchText = searchText.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(lowerSearchText) ||
                    c.Description.ToLower().Contains(lowerSearchText) ||
                    c.Notes.ToLower().Contains(lowerSearchText) ||
                    c.Link.ToLower().Contains(lowerSearchText)
                );
            }

            // 3. Add Item Type filter (if provided)
            if (!string.IsNullOrWhiteSpace(itemType) && itemType.ToLower() != "all")
            {
                query = query.Where(c => c.ItemType == itemType);
            }

            // 4. Add Tag filter (if provided)
            if (tags != null && tags.Any())
            {
                var lowerTags = tags.Select(t => t.ToLower()).ToList();
                var tagCount = lowerTags.Count;

                if (tagMode.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    // EXCLUSIVE (AND) search: The item must have ALL the specified tags.
                    // We do this by checking if the count of matching tags on an item
                    // is equal to the total number of tags we are searching for.
                    query = query.Where(c => c.Tags.Count(itemTag => lowerTags.Contains(itemTag.Name)) == tagCount);
                }
                else
                {
                    // INCLUSIVE (OR) search: The item must have AT LEAST ONE of the specified tags.
                    // (This is the logic you already had).
                    query = query.Where(c => c.Tags.Any(itemTag => lowerTags.Contains(itemTag.Name)));
                }
            }

            // 5. Execute the final, dynamically built query and map to DTOs.
            var results = await query.OrderByDescending(c => c.CreatedOn)
                                     .Select(c => new CubedItemDto
                                     {
                                         // ... mapping logic from your GetMyCubedItems method ...
                                         Id = c.Id,
                                         Name = c.Name,
                                         Link = c.Link,
                                         Description = c.Description,
                                         Notes = c.Notes,
                                         ItemType = c.ItemType,
                                         CreatedOn = c.CreatedOn,
                                         CreatedById = c.CreatedById,
                                         Tags = c.Tags.Select(t => t.Name).ToList()
                                     })
                                     .ToListAsync();

            return Ok(results);
        }
    }
}
