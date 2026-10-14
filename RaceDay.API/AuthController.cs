using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Dto;
using RaceDay.API.Models;

namespace RaceDay.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context) => _context = context;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
                return BadRequest("Username or email already exists.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create Organiser or Participant record
            if (dto.Role == "Organiser")
            {
                _context.Organisers.Add(new Organiser { UserID = user.UserID });
            }
            else if (dto.Role == "Participant")
            {
                _context.Participants.Add(new Participant { UserID = user.UserID });
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = user.UserID }, new { user.UserID, user.Username, user.Role });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            HttpContext.Session.SetUser(user.UserID, user.Role);
            user.LastLoginDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { user.UserID, user.Username, user.Role });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok("Logged out.");
        }
    }
}
