
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
[SessionAuthorize]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly AzureBlobStorageService _blobStorageService;

    public EventsController(
        RaceDayDbContext db,
        AzureBlobStorageService blobStorageService)
    {
        _db = db;
        _blobStorageService = blobStorageService;
    }

    private int CurrentUserId =>
        HttpContext.Session.GetInt32("UserID")!.Value;


    // GET: api/events
    // Get all events with optional filters.

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? type,
        [FromQuery] DateTime? date,
        [FromQuery] string? location)
    {
        var query = _db.Events.AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(e => e.EventType == type);
        }

        if (date.HasValue)
        {
            query = query.Where(
                e => e.EventDate.Date == date.Value.Date);
        }

        if (!string.IsNullOrEmpty(location))
        {
            query = query.Where(
                e => e.Location.Contains(location));
        }

        var events = await query.Select(e => new EventResponseDto(
            e.EventID,
            e.OrganiserID,
            e.Name,
            e.Description,
            e.EventDate,
            e.Location,
            e.Distance,
            e.EventType,
            e.BannerImageUrl,
            e.MaxParticipants,
            e.RegistrationDeadline,
            e.IsActive
        )).ToListAsync();

        return Ok(events);
    }


    // GET: api/events/{id}
    // Get an individual event.

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var ev = await _db.Events.FindAsync(id);

        if (ev == null)
        {
            return NotFound("Event not found.");
        }

        var response = new EventResponseDto(
            ev.EventID,
            ev.OrganiserID,
            ev.Name,
            ev.Description,
            ev.EventDate,
            ev.Location,
            ev.Distance,
            ev.EventType,
            ev.BannerImageUrl,
            ev.MaxParticipants,
            ev.RegistrationDeadline,
            ev.IsActive
        );

        return Ok(response);
    }


    // POST: api/events
    // Create a new event.
    // Only organisers can create events.

    [HttpPost]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> Create(EventCreateDto dto)
    {
        var organiser = await _db.Organisers
            .FirstOrDefaultAsync(
                o => o.UserID == CurrentUserId);

        if (organiser == null)
        {
            return Forbid();
        }

        var ev = new Event
        {
            OrganiserID = organiser.OrganiserID,
            Name = dto.Name,
            Description = dto.Description,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Distance = dto.Distance,
            EventType = dto.EventType,
            BannerImageUrl = dto.BannerImageURL,
            MaxParticipants = dto.MaxParticipants,
            RegistrationDeadline = dto.RegistrationDeadline
        };

        _db.Events.Add(ev);

        await _db.SaveChangesAsync();

        var response = new EventResponseDto(
            ev.EventID,
            ev.OrganiserID,
            ev.Name,
            ev.Description,
            ev.EventDate,
            ev.Location,
            ev.Distance,
            ev.EventType,
            ev.BannerImageUrl,
            ev.MaxParticipants,
            ev.RegistrationDeadline,
            ev.IsActive
        );

        return CreatedAtAction(
            nameof(Get),
            new { id = ev.EventID },
            response);
    }


    // PUT: api/events/{id}
    // Update an existing event.
    // Only the organiser who owns the event can update it.

    [HttpPut("{id}")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> Update(
        int id,
        EventUpdateDto dto)
    {
        var ev = await _db.Events.FindAsync(id);

        if (ev == null)
        {
            return NotFound("Event not found.");
        }

        var organiser = await _db.Organisers
            .FirstOrDefaultAsync(
                o => o.UserID == CurrentUserId);

        if (organiser == null ||
            ev.OrganiserID != organiser.OrganiserID)
        {
            return Forbid();
        }

        ev.Name = dto.Name;
        ev.Description = dto.Description;
        ev.EventDate = dto.EventDate;
        ev.Location = dto.Location;
        ev.Distance = dto.Distance;
        ev.EventType = dto.EventType;
        ev.BannerImageUrl = dto.BannerImageURL;
        ev.MaxParticipants = dto.MaxParticipants;
        ev.RegistrationDeadline = dto.RegistrationDeadline;

        await _db.SaveChangesAsync();

        var response = new EventResponseDto(
            ev.EventID,
            ev.OrganiserID,
            ev.Name,
            ev.Description,
            ev.EventDate,
            ev.Location,
            ev.Distance,
            ev.EventType,
            ev.BannerImageUrl,
            ev.MaxParticipants,
            ev.RegistrationDeadline,
            ev.IsActive
        );

        return Ok(response);
    }


    // POST: api/events/{id}/banner
    // Upload an event banner to Azure Blob Storage.
    // Only the organiser who owns the event can upload a banner.

    [HttpPost("{id}/banner")]
    [RoleAuthorize("Organiser")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadBanner(
        int id,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please select an image.");
        }

        var ev = await _db.Events.FindAsync(id);

        if (ev == null)
        {
            return NotFound("Event not found.");
        }

        var organiser = await _db.Organisers
            .FirstOrDefaultAsync(
                o => o.UserID == CurrentUserId);

        if (organiser == null ||
            ev.OrganiserID != organiser.OrganiserID)
        {
            return Forbid();
        }

        try
        {
            var imageUrl = await _blobStorageService
                .UploadImageAsync(file);

            ev.BannerImageUrl = imageUrl;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Event banner uploaded successfully.",
                eventId = ev.EventID,
                bannerImageUrl = ev.BannerImageUrl
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Problem(
                title: "The banner could not be uploaded.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }


    // DELETE: api/events/{id}
    // Delete an event.
    // Only the organiser who owns the event can delete it.

    [HttpDelete("{id}")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);

        if (ev == null)
        {
            return NotFound("Event not found.");
        }

        var organiser = await _db.Organisers
            .FirstOrDefaultAsync(
                o => o.UserID == CurrentUserId);

        if (organiser == null ||
            ev.OrganiserID != organiser.OrganiserID)
        {
            return Forbid();
        }

        _db.Events.Remove(ev);

        await _db.SaveChangesAsync();

        return NoContent();
    }
}


