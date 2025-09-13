using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;
using TrackCubed.Api.Data;
using TrackCubed.Shared.Models;

namespace TrackCubed.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // IMPORTANT: This entire controller requires a valid token
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("onboard")] // POST /api/user/onboard
        [RequiredScope("CubedItems.ReadWrite")] // Ensures token has the right permission
        public async Task<IActionResult> OnboardCurrentUser()
        {
            // The 'oid' claim is the unique, immutable user ID from Entra.
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(entraObjectId))
            {
                return BadRequest("Could not find user identifier in token.");
            }

            // Check if the user already exists in our database
            var user = await _context.ApplicationUsers
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);

            if (user == null)
            {
                // User does not exist, create a new one
                var newUser = new ApplicationUser
                {
                    EntraObjectId = entraObjectId,
                    DisplayName = User.FindFirstValue("name") ?? "N/A",
                    Email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("preferred_username"),
                    DateCreated = DateTime.UtcNow
                };

                _context.ApplicationUsers.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(newUser); // Return the newly created user
            }

            return Ok(user); // User already exists, return their profile
        }

        // DELETE: api/User/wipe-data

        [HttpDelete("wipe-data")]
        public async Task<IActionResult> WipeUserData()
        {
            var entraObjectId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.ApplicationUsers.AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);
            if (user == null) return Unauthorized();

            // 1. Delete links from join table
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE cit FROM CubedItemTag cit INNER JOIN CubedItems ci ON cit.CubedItemsId = ci.Id WHERE ci.CreatedById = {user.Id}");

            // 2. Delete CubedItems
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM CubedItems WHERE CreatedById = {user.Id}");

            // 3. Delete user-owned Tags
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM Tags WHERE UserId = {user.Id}");

            // 4. Delete user-owned custom ItemTypes
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM UserItemTypes WHERE UserId = {user.Id}");

            return Ok(new { Message = "All user data has been wiped successfully." });
        }
    }
}
