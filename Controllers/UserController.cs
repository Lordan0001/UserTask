using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            return await _context.UserRoles.ToListAsync();
        }
        /// <summary>
        /// Get list of roles using pagination
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("Role")]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles(
                  [FromQuery] int? page = 1,
                  [FromQuery] int? pageSize = 100,
                  [FromQuery, Required] string? sortField = "Id or Name",
                  [FromQuery] string? sortOrder = "asc or desc",
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

            return Ok(result);
        }

      /// <summary>
      /// Get list of users using pagination
      /// </summary>
      /// <param name="page"></param>
      /// <param name="pageSize"></param>
      /// <param name="sortField"></param>
      /// <param name="sortOrder"></param>
      /// <param name="filter"></param>
      /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(
            [FromQuery] int? page = 1,          
            [FromQuery] int? pageSize = 100,    
            [FromQuery,Required] string? sortField = "Id, Name, Age or Email", 
            [FromQuery] string? sortOrder = "asc or desc", 
            [FromQuery] string? filter = null) 
        {
            if (_context.Users == null)
            {
                return NotFound();
            }

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filter) && int.TryParse(filter, out var ageFilter))//Filter
            {
                query = query.Where(user =>
                    user.Name.Contains(filter) ||
                    user.Email.Contains(filter) ||
                    user.Age == ageFilter);
            }
            else if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(user =>
                    user.Name.Contains(filter) ||
                    user.Email.Contains(filter));
            }

        //Sort
            if (!string.IsNullOrEmpty(sortField))
            {
                
                query = sortOrder.ToLower() == "desc" ?
                    query.OrderByDescending(user => EF.Property<int>(user, sortField)) :
                    query.OrderBy(user => EF.Property<int>(user, sortField));
            }


            //Pagination
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

            return userWithRoles;
        }


        // Post
        /// <summary>
        /// Create user 
        /// </summary>
        /// <param name="createUser"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(DTOUser createUser)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Users == null)
            {
                return Problem("Entity set 'MainContext.Users'  is null.");
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
            return user;
        }
        /// <summary>
        /// Give a role to user
        /// </summary>
        /// <param name="userRole"></param>
        /// <returns></returns>
        [HttpPost("Role")]
        public async Task<ActionResult<UserRole>> PostUserRole(UserRole userRole)
        {
            if (_context.UserRoles == null)
            {
                return Problem("Entity set 'MainContext.UserRoles'  is null.");
            }
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

        
            return Ok("Added role with id: " +userRole.RoleId+ " to user with id: " + userRole.UserId);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var existingUser = await _context.Users.FindAsync(id);

            if (existingUser == null)
            {
                return NotFound();
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
            return Ok(existingUser);
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

            return Ok("user with id: " +id + " was deleted!");
        }

        /// <summary>
        /// Delete record UserRoles by id (if need to remove role from user)
        /// </summary>
        [HttpDelete("UserRoles/{id}")]

        public async Task<IActionResult> DeleteUserRole(int id)
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

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
