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
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MainContext _context;
        private const int DEFAULTUSERROLE = 4;//user role id

        public UserController(MainContext context)
        {
            _context = context;
        }

        // GET
        [HttpGet("UserRoles")]
        public async Task<ActionResult<IEnumerable<UserRole>>> GetUserRoles()
        {
            if (_context.UserRoles == null)
            {
                return NotFound();
            }
            return await _context.UserRoles.ToListAsync();
        }
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
