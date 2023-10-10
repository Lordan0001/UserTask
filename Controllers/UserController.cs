using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserTask.Context;
using UserTask.ControllerModels;
using UserTask.Models;

namespace UserTask.Controllers
{
    /// <summary>
    /// Controller for user CRUD add give/remove them roles
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    
    public class UserController : ControllerBase
    {
        private readonly MainContext _context;
        private const int DEFAULTUSERROLE = 4;//user role id
        /// <summary>
        /// init main context for db data
        /// </summary>
        /// <param name="context"></param>

        public UserController(MainContext context)
        {
            _context = context;
        }

        // GET
        /// <summary>
        /// Get list of all roles
        /// </summary>
        /// <returns></returns>
        [HttpGet("UserRoles")]
        public async Task<ActionResult<IEnumerable<UserRole>>> GetUserRoles()
        {
            if (_context.UserRoles == null)
            {
                return NotFound();
            }
            Log.Information("UserController get user role => {@Userroles}", await _context.UserRoles.ToListAsync());
            return await _context.UserRoles.ToListAsync();
        }
        /// <summary>
        /// Get a list of roles with pagination support.
        /// </summary>
        /// <param name="page">Page number (default is 1).</param>
        /// <param name="pageSize">Number of items per page (default is 100).</param>
        /// <param name="sortField">Field to sort by  (Id or Name).</param>
        /// <param name="sortOrder">Sorting order (asc or desc).</param>
        /// <param name="filter">Filter users by keyword.</param>
        [HttpGet("Role")]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles(
                  [FromQuery] int? page = 1,
                  [FromQuery] int? pageSize = 100,
                  [FromQuery, Required] string? sortField = "Id",
                  [FromQuery] string? sortOrder = "asc",
                  [FromQuery] string? filter = null)
        {
            if (_context.Roles == null)
            {
                return NotFound();
            }

            var query = _context.Roles.AsQueryable();

            //filter
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(role =>
                    role.Name.Contains(filter));
            }

            // Sort
            if (!string.IsNullOrEmpty(sortField))
            {
                query = sortOrder.ToLower() == "desc" ?
                    query.OrderByDescending(role => EF.Property<int>(role, sortField)) :
                    query.OrderBy(role => EF.Property<int>(role, sortField));
            }

            // Pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize.GetValueOrDefault());
            var currentPage = page.GetValueOrDefault();
            var items = await query
                .Skip((currentPage - 1) * pageSize.GetValueOrDefault())
                .Take(pageSize.GetValueOrDefault())
                .ToListAsync();

            var result = new
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize.GetValueOrDefault(),
                Items = items
            };
            Log.Information("UserController: Retrieving roles with pagination. Total roles found: {TotalRoles}, Page: {Page}, PageSize: {PageSize}", totalItems, currentPage, pageSize);


            return Ok(result);
        }

        /// <summary>
        /// Get a list of users with pagination support.
        /// </summary>
        /// <param name="page">Page number (default is 1).</param>
        /// <param name="pageSize">Number of items per page (default is 100).</param>
        /// <param name="sortField">Field to sort by (e.g., "Id", "Name", "Age", "Email").</param>
        /// <param name="sortOrder">Sorting order ("asc" or "desc").</param>
        /// <param name="filter">Filter users by keyword.</param>
        /// <returns>A list of users with pagination details.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(
            [FromQuery, Range(1, int.MaxValue)] int page = 1,
            [FromQuery, Range(1, int.MaxValue)] int pageSize = 100,
            [FromQuery] string sortField = "Id",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] string? filter = null)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filter) && int.TryParse(filter, out var ageFilter))
            {
                // Filter by keyword or age if a valid number is provided
                query = query.Where(user =>
                    user.Name.Contains(filter) ||
                    user.Email.Contains(filter) ||
                    user.Age == ageFilter);
            }
            else if (!string.IsNullOrEmpty(filter))
            {
                // Filter by keyword only
                query = query.Where(user =>
                    user.Name.Contains(filter) ||
                    user.Email.Contains(filter));
            }

            if (!string.IsNullOrEmpty(sortField))
            {
                // Sort by the specified field and order
                query = sortOrder.ToLower() == "desc" ?
                    query.OrderByDescending(user => EF.Property<int>(user, sortField)) :
                    query.OrderBy(user => EF.Property<int>(user, sortField));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var currentPage = page;

            var items = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = items
            };
            Log.Information("UserController: Retrieving users with pagination. Total roles found: {TotalRoles}, Page: {Page}, PageSize: {PageSize}", totalItems, currentPage, pageSize);


            return Ok(result);
        }

        /// <summary>
        /// Get user info with role using user id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUserWithRoles(int id)
        {
            if (_context.Users == null || _context.UserRoles == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _context.UserRoles
                .Where(usr => usr.UserId == id)
                .Select(usr => usr.RoleId)
                .ToListAsync();

            var roles = await _context.Roles
                .Where(role => userRoles.Contains(role.Id))
                .ToListAsync();

            var userWithRoles = new
            {
                User = user,
                Roles = roles
            };
            Log.Information("UserController: get user with rols => {@userWithRoles}", userWithRoles);


            return userWithRoles;
        }


        // Post
        /// <summary>
        /// Create a user 
        /// </summary>
        /// <param name="createUser"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(DTOUser createUser)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (_context.Users == null)
                {
                    return Problem("Entity set 'MainContext.Users' is null.");
                }

                if (_context.Users.Any(u => u.Email == createUser.email))
                {
                    ModelState.AddModelError("email", "Email is already in use.");
                    return BadRequest(ModelState);
                }

                var user = new User
                {
                    Name = createUser.name,
                    Age = createUser.age,
                    Email = createUser.email
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = DEFAULTUSERROLE
                };
                _context.UserRoles.Add(userRole);

                await _context.SaveChangesAsync();

                Log.Information("UserController: add user => {@user}", user);

                return user;
            }
            catch (DbUpdateException ex)
            {
                // Handle database update error
                Log.Error(ex, "Error while creating user.");
                return StatusCode(500, "An error occurred while creating the user.");
            }
        }

        /// <summary>
        /// Give a role to user
        /// </summary>
        /// <param name="dtoUserRole"></param>
        /// <returns></returns>
        [HttpPost("Role")]
        public async Task<ActionResult<UserRole>> PostUserRole(DTOUserRole dtoUserRole)
        {
            try
            {
                if (_context.UserRoles == null)
                {
                    return Problem("Entity set 'MainContext.UserRoles' is null.");
                }

                var userRole = new UserRole
                {
                    UserId = dtoUserRole.UserId,
                    RoleId = dtoUserRole.RoleId,
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();

                Log.Information("UserController: add role => {@userRole}", userRole);

                return Ok("Added role with id: " + userRole.RoleId + " to user with id: " + userRole.UserId);
            }
            catch (DbUpdateException ex)
            {
            
                Log.Error(ex, "Error while adding role to user.");
                return StatusCode(500, "An error occurred while adding the role to the user.");
            }
        }

        // Put
        /// <summary>
        /// Update user info 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updatedUser"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, DTOUser updatedUser)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingUser = await _context.Users.FindAsync(id);

                if (existingUser == null)
                {
                    return NotFound();
                }

                if (await _context.Users.AnyAsync(u => u.Email == updatedUser.email && u.Id != id))
                {
                    ModelState.AddModelError("email", "Email is already in use.");
                    return BadRequest(ModelState);
                }

                existingUser.Name = updatedUser.name;
                existingUser.Age = updatedUser.age;
                existingUser.Email = updatedUser.email;

                _context.Entry(existingUser).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                Log.Information("UserController: update user info by id => {@existingUser}", existingUser);

                return Ok(existingUser);
            }
            catch (DbUpdateException ex)
            {
                // Handle database update error
                Log.Error(ex, "Error while updating user info.");
                return StatusCode(500, "An error occurred while updating user info.");
            }
        }




        // Delete
        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                if (_context.Users == null)
                {
                    return NotFound();
                }

                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                Log.Information("UserController: delete user by id => {@user}", user);

                return Ok("User with id: " + id + " was deleted!");
            }
            catch (DbUpdateException ex)
            {
  
                Log.Error(ex, "Error while deleting user.");
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }


        /// <summary>
        /// Delete record UserRoles by id (if need to remove role from user)
        /// </summary>
        [HttpDelete("UserRoles/{id}")]
        public async Task<IActionResult> DeleteUserRole(int id)
        {
            try
            {
                if (_context.UserRoles == null)
                {
                    return NotFound();
                }

                var userRole = await _context.UserRoles.FindAsync(id);

                if (userRole == null)
                {
                    return NotFound();
                }

                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
                Log.Information("UserController: delete role from user by id => {@userRole}", userRole);

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "Error while deleting user role.");
                return StatusCode(500, "An error occurred while deleting the user role.");
            }
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
