using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Attributes;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using RaceDay.API.Controllers;
using RaceDay.API.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db) => _db = db;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username))
            return BadRequest("Username or email already exists.");

        if (dto.Role != "Organiser" && dto.Role != "Participant")
            return BadRequest("Role must be Organiser or Participant.");

        var (hash, salt) = PasswordHasher.CreateHash(dto.Password);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber,
            Role = dto.Role
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        if (dto.Role == "Organiser")
            _db.Organisers.Add(new Organiser { UserID = user.UserID });
        else
            _db.Participants.Add(new Participant { UserID = user.UserID });

        await _db.SaveChangesAsync();

        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);

        return CreatedAtAction(nameof(ProfileController.Get), null,
            new UserResponseDto(user.UserID, user.Username, user.Email,
                user.FirstName, user.LastName, user.Role));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);

        if (user == null || !user.IsActive)
            return Unauthorized("Invalid credentials.");

        if (!PasswordHasher.Verify(dto.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized("Invalid credentials.");

        user.LastLoginDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);

        return Ok(new UserResponseDto(user.UserID, user.Username, user.Email,
            user.FirstName, user.LastName, user.Role));
    }

    [HttpPost("logout")]
    [SessionAuthorize]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok("Logout successful.");
    }
}
