
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Attributes;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
[SessionAuthorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    // Get the ID of the logged-in user
    private int CurrentUserId =>
        HttpContext.Session.GetInt32("UserID")!.Value;

    // POST: api/events/{eventId}/enrol

    [HttpPost("events/{eventId}/enrol")]
    [RoleAuthorize("Participant")]
    public async Task<IActionResult> Enrol(
        int eventId, EnrolDto dto)
    {
        // Find the participant linked to the logged-in user
        var participant = await _db.Participants
            .FirstOrDefaultAsync(p => p.UserID == CurrentUserId);

        if (participant == null)
        {
            return StatusCode(
                403,
                "Your account does not have a Participant record.");
        }

        // Check whether the event exists
        var ev = await _db.Events.FindAsync(eventId);

        if (ev == null)
        {
            return NotFound("Event not found.");
        }

        // Check whether the category belongs to this event
        var category = await _db.Categories
            .FirstOrDefaultAsync(c =>
                c.CategoryID == dto.CategoryId &&
                c.EventID == eventId);

        if (category == null)
        {
            return NotFound("Category not found for this event.");
        }

        // Prevent duplicate enrolments
        bool alreadyEnrolled = await _db.EventEnrolments
            .AnyAsync(ee =>
                ee.ParticipantID == participant.ParticipantID &&
                ee.EventID == eventId);

        if (alreadyEnrolled)
        {
            return BadRequest("Already enrolled in this event.");
        }

        // Create the enrolment
        var enrolment = new EventEnrolment
        {
            ParticipantID = participant.ParticipantID,
            EventID = eventId,
            CategoryID = dto.CategoryId,
            Status = "Pending",
            PaymentStatus = "Pending"
        };

        // Save the enrolment
        _db.EventEnrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        // Return HTTP 201 Created with the enrolment details
        return CreatedAtAction(
            nameof(GetMyEnrolments),
            null,
            new EnrolmentResponseDto(
                enrolment.EnrolmentID,
                enrolment.ParticipantID,
                enrolment.EventID,
                enrolment.CategoryID,
                enrolment.EnrolmentDate,
                enrolment.Status,
                enrolment.PaymentStatus,
                enrolment.BibNumber,
                enrolment.Notes
            )
        );
    }


    // GET: api/events/{eventId}/enrolments
    [HttpGet("events/{eventId}/enrolments")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> GetEventEnrolments(
        int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);

        if (ev == null)
        {
            return NotFound("Event not found.");
        }

        var organiser = await _db.Organisers
            .FirstOrDefaultAsync(o =>
                o.UserID == CurrentUserId);

        if (organiser == null ||
            ev.OrganiserID != organiser.OrganiserID)
        {
            return StatusCode(403,
                "You are not authorised to view these enrolments.");
        }

        var enrolments = await _db.EventEnrolments
            .Where(ee => ee.EventID == eventId)
            .Include(ee => ee.Participant)
                .ThenInclude(p => p.User)
            .Include(ee => ee.Category)
            .Select(ee => new
            {
                ee.EnrolmentID,
                ParticipantName =
                    ee.Participant.User.FirstName + " " +
                    ee.Participant.User.LastName,
                ee.Participant.User.Email,
                Category = ee.Category.Name,
                ee.Status,
                ee.PaymentStatus,
                ee.BibNumber,
                ee.EnrolmentDate
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // GET: api/participants/enrolments
    [HttpGet("participants/enrolments")]
    [RoleAuthorize("Participant")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var participant = await _db.Participants
            .FirstOrDefaultAsync(p =>
                p.UserID == CurrentUserId);

        if (participant == null)
        {
            return StatusCode(403,
                "Your account does not have a Participant record.");
        }

        var enrolments = await _db.EventEnrolments
            .Where(ee =>
                ee.ParticipantID == participant.ParticipantID)
            .Include(ee => ee.Event)
            .Include(ee => ee.Category)
            .Select(ee => new
            {
                ee.EnrolmentID,
                Event = ee.Event.Name,
                ee.Event.EventDate,
                Category = ee.Category.Name,
                ee.Status,
                ee.PaymentStatus,
                ee.BibNumber
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // PUT: api/enrolments/{id}/status
    [HttpPut("enrolments/{id}/status")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> UpdateStatus(
        int id, EnrolmentStatusUpdateDto dto)
    {
        var enrolment = await _db.EventEnrolments
            .Include(ee => ee.Event)
            .FirstOrDefaultAsync(ee =>
                ee.EnrolmentID == id);

        if (enrolment == null)
        {
            return NotFound("Enrolment not found.");
        }

        var organiser = await _db.Organisers
            .FirstOrDefaultAsync(o =>
                o.UserID == CurrentUserId);

        if (organiser == null ||
            enrolment.Event.OrganiserID != organiser.OrganiserID)
        {
            return StatusCode(403,
                "You are not authorised to update this enrolment.");
        }

        // Validate the requested enrolment status
        if (dto.Status != "Pending" &&
            dto.Status != "Confirmed" &&
            dto.Status != "Cancelled" &&
            dto.Status != "Completed")
        {
            return BadRequest("Invalid status.");
        }

        enrolment.Status = dto.Status;

        await _db.SaveChangesAsync();

        return Ok(new EnrolmentResponseDto(
            enrolment.EnrolmentID,
            enrolment.ParticipantID,
            enrolment.EventID,
            enrolment.CategoryID,
            enrolment.EnrolmentDate,
            enrolment.Status,
            enrolment.PaymentStatus,
            enrolment.BibNumber,
            enrolment.Notes
        ));
    }
}

