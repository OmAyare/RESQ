using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESQ_API.Data;
using RESQ_API.Domain.Entities;

namespace RESQ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RESQ_DbContext _resq_dbcontext;

        public UsersController(RESQ_DbContext resq_dbcontext) => _resq_dbcontext = resq_dbcontext;

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            return await _resq_dbcontext.Users.ToListAsync();
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _resq_dbcontext.Users.FindAsync(id);
            if (user == null) return NotFound();
            return user;
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {

            var user = new User
            {
                FullName = dto.FullName,
                UserName = dto.UserName,
                PersonalPhoneNumber = dto.PersonalPhoneNumber,
                FamilyPhoneNumber = dto.FamilyPhoneNumber,
                Region = dto.Region,
                District = dto.District
            };

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"❌ ModelState Errors: {string.Join(", ", errors)}");
                return BadRequest(ModelState);
            }

            // Check for duplicate email
            var exists = await _resq_dbcontext.Users.AnyAsync(u => u.UserName == user.UserName);
            if (exists)
            {
                return BadRequest("Email is already registered.");
            }


            _resq_dbcontext.Users.Add(user);
            await _resq_dbcontext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
        
        
        
        
        
        
        
        
        
        
        
        
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            if (id != updatedUser.UserId)
            {
                return BadRequest("User ID mismatch.");
            }
            var user = await _resq_dbcontext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update allowed fields
            user.FullName = updatedUser.FullName;
            var exists = await _resq_dbcontext.Users.AnyAsync(u => u.UserName == user.UserName);
            if (exists)
            {
                return BadRequest("Email is already registered.");
            }
            else 
            {
                user.UserName = updatedUser.UserName;
            }
            user.PersonalPhoneNumber = updatedUser.PersonalPhoneNumber;
            user.FamilyPhoneNumber = updatedUser.FamilyPhoneNumber;
            user.Region = updatedUser.Region;
            user.District = updatedUser.District;

        
            await _resq_dbcontext.SaveChangesAsync();
            return NoContent(); // 204
        }

    }
}
