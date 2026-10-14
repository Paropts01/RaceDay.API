using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Attributes;
using RaceDay.API.Data;
using RaceDay.API.DTOs;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[SessionAuthorize]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ProfileController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => HttpContext.Session.GetInt32("UserID")!.Value;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var user = await _db.Users
            .Include(u => u.Organiser)
            .Include(u => u.Participant)
            .FirstOrDefaultAsync(u => u.UserID == CurrentUserId);

        if (user == null) return NotFound();

        return Ok(new
        {
            user.UserID,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.DateOfBirth,
            user.PhoneNumber,
            user.Role,
            Organiser = user.Organiser == null ? null : new
            {
                user.Organiser.OrganisationName,
                user.Organiser.Bio,
                user.Organiser.WebsiteURL
            },
            Participant = user.Participant == null ? null : new
            {
                user.Participant.ProfilePictureURL,
                user.Participant.EmergencyContactName,
                user.Participant.EmergencyContactPhone,
                user.Participant.MedicalConditions
            }
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update(ProfileUpdateDto dto)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        user.DateOfBirth = dto.DateOfBirth;

        await _db.SaveChangesAsync();
        return Ok("Profile updated.");
    }
}

public record ProfileUpdateDto(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateTime DateOfBirth
);
