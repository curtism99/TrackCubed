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
                  .Where(c => c.CreatedById == user.Id)
                  .OrderByDescending(c => c.CreatedOn)
                  .Select(c => new CubedItemDto
                  {
                      Id = c.Id,
                      Name = c.Name,
                      Link = c.Link,
                      Description = c.Description,
                      Notes = c.Notes,
                      ItemTypeName = c.ItemType.Name, // <-- THE FIX: Map from the navigation property
                      CreatedOn = c.CreatedOn,
                      CreatedById = c.CreatedById,
                      Tags = c.Tags.Select(t => t.Name).ToList()
                  })
                  .ToListAsync();
            return Ok(items);
        }


        // POST: api/CubedItems
        [HttpPost]
        public async Task<ActionResult<CubedItemDto>> CreateCubedItem(CubedItemCreateDto itemDto)
        {
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null) return Unauthorized("User profile not found.");

            // This returns the string name, but also creates the type if it's a new custom one.
            int itemTypeId = await ProcessItemTypeId(itemDto.ItemTypeName, user.Id);

            var newCubedItem = new CubedItem
            {
                Id = Guid.NewGuid(),
                Name = itemDto.Name,
                Link = itemDto.Link,
                Description = itemDto.Description,
                Notes = itemDto.Notes,
                ItemTypeId = itemTypeId,
                CreatedById = user.Id,
                CreatedOn = DateTime.UtcNow,
                DateLastAccessed = DateTime.UtcNow
            };

            await UpdateItemTags(newCubedItem, itemDto.Tags);

            _context.CubedItems.Add(newCubedItem);
            await _context.SaveChangesAsync();

            // --- The Fix Is Here ---
            // We already have all the information we need. No need for another database call.
            var createdItemDto = new CubedItemDto
            {
                Id = newCubedItem.Id,
                Name = newCubedItem.Name,
                Link = newCubedItem.Link,
                Description = newCubedItem.Description,
                Notes = newCubedItem.Notes,
                ItemTypeName = itemDto.ItemTypeName, // Use the name we already processed!
                CreatedOn = newCubedItem.CreatedOn,
                CreatedById = newCubedItem.CreatedById,
                Tags = itemDto.Tags // The tags from the original request are correct
            };

            return CreatedAtAction(nameof(GetMyCubedItems), new { id = createdItemDto.Id }, createdItemDto);
        }


        // DELETE: api/CubedItems/1234-5678-9012-3456
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCubedItem(Guid id)
        {
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null)
            {
                return Unauthorized();
            }

            // 1. First, verify the item exists AND belongs to the user. This is a critical security check.
            var itemExists = await _context.CubedItems
                                           .AnyAsync(c => c.Id == id && c.CreatedById == user.Id);
            if (!itemExists)
            {
                return NotFound("Item not found or you do not have permission to delete it.");
            }

            // --- The Definitive Deletion Order using Raw SQL ---

            // 2. Manually delete all records from the join table that link to this specific item.
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM CubedItemTag WHERE CubedItemsId = {id}");

            // 3. Now that the links are gone, we can safely delete the item itself.
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM CubedItems WHERE Id = {id}");

            // Note: We do NOT delete the tags themselves, as they might be used by other items.
            // The "Clean Up Orphaned Tags" feature will handle that.

            return NoContent(); // Return "204 No Content" for a successful delete.
        }

        // PUT: api/CubedItems/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCubedItem(Guid id, CubedItemDto itemDto)
        {
            if (id != itemDto.Id)
            {
                return BadRequest("ID mismatch.");
            }

            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null)
            {
                return Unauthorized();
            }

            // 1. Find the existing item AND its related tags in a single query.
            var itemToUpdate = await _context.CubedItems
                                             .Include(i => i.Tags) // Eager loading is crucial here
                                             .FirstOrDefaultAsync(c => c.Id == id && c.CreatedById == user.Id);
            if (itemToUpdate == null)
            {
                return NotFound("Item not found or you do not have permission to edit it.");
            }

            // 2. Process the ItemType name to get its ID, creating it if it's a new custom type.
            int itemTypeId = await ProcessItemTypeName(itemDto.ItemTypeName, user.Id);

            // 3. Update the item's properties from the DTO.
            itemToUpdate.Name = itemDto.Name;
            itemToUpdate.Link = itemDto.Link;
            itemToUpdate.Description = itemDto.Description;
            itemToUpdate.Notes = itemDto.Notes;
            itemToUpdate.ItemTypeId = itemTypeId;
            itemToUpdate.DateLastAccessed = DateTime.UtcNow;

            // 4. Update the tags. The helper method will now work on the in-memory collection.
            //    There is no need for an extra LoadAsync call.
            await UpdateItemTags(itemToUpdate, itemDto.Tags);

            // 5. Save all changes (the item update AND any tag relationship changes).
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task UpdateItemTags(CubedItem item, List<string> tagNames)
        {
            // Clear existing tags to handle removals
            item.Tags.Clear();

            if (tagNames == null || !tagNames.Any()) return;

            // Get the UserID from the item. We need this for all tag lookups.
            var userId = item.CreatedById;

            foreach (var tagName in tagNames.Select(t => t.Trim().ToLower()).Distinct())
            {
                if (string.IsNullOrWhiteSpace(tagName)) continue;

                // Find an existing tag that matches the name AND belongs to the current user.
                var existingTag = await _context.Tags
                                                .FirstOrDefaultAsync(t => t.Name == tagName && t.UserId == userId);
                if (existingTag != null)
                {
                    item.Tags.Add(existingTag);
                }
                else
                {
                    // This tag is new FOR THIS USER.
                    item.Tags.Add(new Tag { Name = tagName, UserId = userId });
                }
            }
        }

        // GET: api/CubedItems/search?searchText=...&itemType=...&tags=...
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CubedItemDto>>> SearchMyCubedItems(
            [FromQuery] string? searchText,
            [FromQuery] int? itemTypeId,
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

            // This is now much simpler and faster.
            if (itemTypeId.HasValue && itemTypeId.Value != 0) // We'll treat 0 as "All"
            {
                query = query.Where(c => c.ItemTypeId == itemTypeId.Value);
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
                    Id = c.Id,
                    Name = c.Name,
                    Link = c.Link,
                    Description = c.Description,
                    Notes = c.Notes,
                    ItemTypeName = c.ItemType.Name,
                    CreatedOn = c.CreatedOn,
                    CreatedById = c.CreatedById,
                    Tags = c.Tags.Select(t => t.Name).ToList()
                })
                .ToListAsync();
            return Ok(results);
        }

        private async Task<int> ProcessItemTypeName(string itemTypeName, int userId)
        {
            if (string.IsNullOrWhiteSpace(itemTypeName)) itemTypeName = "Other";

            var existingType = await _context.ItemTypes
                .FirstOrDefaultAsync(it => it.Name.ToLower() == itemTypeName.ToLower() && (it.UserId == null || it.UserId == userId));

            if (existingType != null)
            {
                return existingType.Id;
            }

            var newCustomType = new ItemType { Name = itemTypeName, UserId = userId };
            _context.ItemTypes.Add(newCustomType);
            await _context.SaveChangesAsync(); // Must save here to generate the new ID
            return newCustomType.Id;
        }

        private async Task<int> ProcessItemTypeId(string itemTypeName, int userId)
        {
            if (string.IsNullOrWhiteSpace(itemTypeName))
            {
                itemTypeName = "Other"; // Default to "Other"
            }

            // Find if the type exists (either system or for this user)
            var existingType = await _context.ItemTypes
                .FirstOrDefaultAsync(it => it.Name.ToLower() == itemTypeName.ToLower() &&
                                           (it.UserId == null || it.UserId == userId));

            // If it exists, return its ID
            if (existingType != null)
            {
                return existingType.Id;
            }

            // If it's a new custom type, create it, save it, and return the new ID
            var newCustomType = new ItemType { Name = itemTypeName, UserId = userId };
            _context.ItemTypes.Add(newCustomType);
            await _context.SaveChangesAsync(); // Save here to generate the new ID
            return newCustomType.Id;
        }
    }
}
