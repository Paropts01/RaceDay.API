
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Attributes;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public AuthController(RaceDayDbContext db)
    {
        _db = db;
    }

    // POST: api/Auth/register

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // Check if the username or email already exists
        if (await _db.Users.AnyAsync(u =>
            u.Email == dto.Email || u.Username == dto.Username))
        {
            return BadRequest("Username or email already exists.");
        }

        // Validate the user's role
        if (dto.Role != "Organiser" && dto.Role != "Participant")
        {
            return BadRequest("Role must be Organiser or Participant.");
        }

        // Hash the password
        var (hash, salt) = PasswordHasher.CreateHash(dto.Password);

        // Create the user
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

        // Save the user
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Create the Organiser or Participant record
        if (dto.Role == "Organiser")
        {
            _db.Organisers.Add(new Organiser
            {
                UserID = user.UserID
            });
        }
        else
        {
            _db.Participants.Add(new Participant
            {
                UserID = user.UserID
            });
        }

        // Save the role-specific record
        await _db.SaveChangesAsync();

        // Store the user's details in the session
        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);

        // Return HTTP 201 Created with the registered user's details
        return CreatedAtAction(
            nameof(Register),
            new { },
            new UserResponseDto(
                user.UserID,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role
            )
        );
    }


    // POST: api/Auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        // Find the user by username or email
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username == dto.UsernameOrEmail ||
            u.Email == dto.UsernameOrEmail);

        // Check whether the account exists and is active
        if (user == null || !user.IsActive)
        {
            return Unauthorized("Invalid credentials.");
        }

        // Verify the password
        if (!PasswordHasher.Verify(
            dto.Password,
            user.PasswordHash,
            user.PasswordSalt))
        {
            return Unauthorized("Invalid credentials.");
        }

        // Update the last login date
        user.LastLoginDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Store the user's details in the session
        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);

        // Return the logged-in user's details
        return Ok(new UserResponseDto(
            user.UserID,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role
        ));
    }

    // POST: api/Auth/logout
    [HttpPost("logout")]
    [SessionAuthorize]
    public IActionResult Logout()
    {
        // Clear the current session
        HttpContext.Session.Clear();

        return Ok("Logout successful.");
    }
}
