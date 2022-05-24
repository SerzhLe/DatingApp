using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        //these policy are created in identityserviceextension
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRolesAsync()
        {
            var users = await _userManager.Users
                .Include(r => r.UserRoles) //join with AppUserRole table
                .ThenInclude(r => r.Role) //then join with AppRole table
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    UserName = u.UserName,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRole(string username, [FromQuery] string roles)
        {
            if (roles == null) return BadRequest("Member must have at least one role");

            var selectedRoles = roles.Split(',').ToArray(); //roles will go separated by comma

            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return NotFound("Could not find user");

            var userRoles = await _userManager.GetRolesAsync(user);

            if (Enumerable.SequenceEqual(selectedRoles, userRoles)) return BadRequest("User already has these roles");

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles)); //will add different roles from userRoles

            if (!result.Succeeded) return BadRequest(result.Errors);

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles)); //remove all diference from selectedRoles

            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}