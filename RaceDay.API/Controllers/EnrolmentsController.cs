using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Attributes;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[SessionAuthorize]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventsController(AppDbContext db) => _db = db;

    private int CurrentUserId => HttpContext.Session.GetInt32("UserID")!.Value;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] DateTime? date, [FromQuery] string? location)
    {
        var query = _db.Events.AsQueryable();

        if (!string.IsNullOrEmpty(type))
            query = query.Where(e => e.EventType == type);
        if (date.HasValue)
            query = query.Where(e => e.EventDate.Date == date.Value.Date);
        if (!string.IsNullOrEmpty(location))
            query = query.Where(e => e.Location.Contains(location));

        var events = await query
            .Select(e => new EventResponseDto(
                e.EventID, e.OrganiserID, e.Name, e.Description, e.EventDate,
                e.Location, e.Distance, e.EventType, e.BannerImageURL,
                e.MaxParticipants, e.RegistrationDeadline, e.IsActive))
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();

        return Ok(new EventResponseDto(
            ev.EventID, ev.OrganiserID, ev.Name, ev.Description, ev.EventDate,
            ev.Location, ev.Distance, ev.EventType, ev.BannerImageURL,
            ev.MaxParticipants, ev.RegistrationDeadline, ev.IsActive));
    }

    [HttpPost]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> Create(EventCreateDto dto)
    {
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserId);
        if (organiser == null) return Forbid();

        var ev = new Event
        {
            OrganiserID = organiser.OrganiserID,
            Name = dto.Name,
            Description = dto.Description,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Distance = dto.Distance,
            EventType = dto.EventType,
            BannerImageURL = dto.BannerImageURL,
            MaxParticipants = dto.MaxParticipants,
            RegistrationDeadline = dto.RegistrationDeadline
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = ev.EventID },
            new EventResponseDto(ev.EventID, ev.OrganiserID, ev.Name, ev.Description,
                ev.EventDate, ev.Location, ev.Distance, ev.EventType,
                ev.BannerImageURL, ev.MaxParticipants, ev.RegistrationDeadline, ev.IsActive));
    }

    [HttpPut("{id}")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, EventUpdateDto dto)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserId);
        if (organiser == null || ev.OrganiserID != organiser.OrganiserID)
            return Forbid();

        ev.Name = dto.Name;
        ev.Description = dto.Description;
        ev.EventDate = dto.EventDate;
        ev.Location = dto.Location;
        ev.Distance = dto.Distance;
        ev.EventType = dto.EventType;
        ev.BannerImageURL = dto.BannerImageURL;
        ev.MaxParticipants = dto.MaxParticipants;
        ev.RegistrationDeadline = dto.RegistrationDeadline;

        await _db.SaveChangesAsync();
        return Ok(new EventResponseDto(ev.EventID, ev.OrganiserID, ev.Name, ev.Description,
            ev.EventDate, ev.Location, ev.Distance, ev.EventType,
            ev.BannerImageURL, ev.MaxParticipants, ev.RegistrationDeadline, ev.IsActive));
    }

    [HttpDelete("{id}")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserId);
        if (organiser == null || ev.OrganiserID != organiser.OrganiserID)
            return Forbid();

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
