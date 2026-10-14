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
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => HttpContext.Session.GetInt32("UserID")!.Value;

    [HttpPost("events/{eventId}/results")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> Create(int eventId, ResultCreateDto dto)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound("Event not found.");

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserId);
        if (organiser == null || ev.OrganiserID != organiser.OrganiserID) return Forbid();

        var enrolment = await _db.EventEnrolments.FirstOrDefaultAsync(ee => ee.EnrolmentID == dto.EnrolmentId && ee.EventID == eventId);
        if (enrolment == null) return BadRequest("Enrolment not found for this event.");

        if (await _db.Results.AnyAsync(r => r.EnrolmentID == dto.EnrolmentId))
            return BadRequest("Result already exists for this enrolment.");

        var result = new Result
        {
            EnrolmentID = dto.EnrolmentId,
            FinishTime = dto.FinishTime,
            NetTime = dto.NetTime,
            FinishingPosition = dto.FinishingPosition,
            OverallPlacement = dto.OverallPlacement,
            CategoryPlacement = dto.CategoryPlacement,
            Status = dto.Status,
            Notes = dto.Notes
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEventResults), new { eventId },
            new ResultResponseDto(result.ResultID, result.EnrolmentID, result.FinishTime,
                result.NetTime, result.FinishingPosition, result.OverallPlacement,
                result.CategoryPlacement, result.Status, result.IsPublished, result.Notes));
    }

    [HttpGet("events/{eventId}/results")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound("Event not found.");

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserId);
        if (organiser == null || ev.OrganiserID != organiser.OrganiserID) return Forbid();

        var results = await _db.Results.Include(r => r.Enrolment).ThenInclude(ee => ee.Participant).ThenInclude(p => p.User)
            .Where(r => r.Enrolment.EventID == eventId)
            .Select(r => new
            {
                r.ResultID,
                ParticipantName = r.Enrolment.Participant.User.FirstName + " " + r.Enrolment.Participant.User.LastName,
                r.FinishTime,
                r.NetTime,
                r.FinishingPosition,
                r.OverallPlacement,
                r.CategoryPlacement,
                r.Status,
                r.IsPublished
            }).ToListAsync();

        return Ok(results);
    }

    [HttpGet("participants/results")]
    [RoleAuthorize("Participant")]
    public async Task<IActionResult> GetMyResults()
    {
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserId);
        if (participant == null) return Forbid();

        var results = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(ee => ee.Event)
            .Include(r => r.Enrolment).ThenInclude(ee => ee.Category)
            .Where(r => r.Enrolment.ParticipantID == participant.ParticipantID && r.IsPublished)
            .Select(r => new
            {
                r.ResultID,
                Event = r.Enrolment.Event.Name,
                r.Enrolment.Event.EventDate,
                Category = r.Enrolment.Category.Name,
                r.FinishTime,
                r.NetTime,
                r.FinishingPosition,
                r.OverallPlacement,
                r.CategoryPlacement,
                r.Status
            }).ToListAsync();

        return Ok(results);
    }

    [HttpPut("results/{id}")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, ResultUpdateDto dto)
    {
        var result = await _db.Results.Include(r => r.Enrolment).ThenInclude(ee => ee.Event).FirstOrDefaultAsync(r => r.ResultID == id);
        if (result == null) return NotFound();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserId);
        if (organiser == null || result.Enrolment.Event.OrganiserID != organiser.OrganiserID) return Forbid();

        result.FinishTime = dto.FinishTime;
        result.NetTime = dto.NetTime;
        result.FinishingPosition = dto.FinishingPosition;
        result.OverallPlacement = dto.OverallPlacement;
        result.CategoryPlacement = dto.CategoryPlacement;
        result.Status = dto.Status;
        result.Notes = dto.Notes;

        await _db.SaveChangesAsync();
        return Ok(new ResultResponseDto(result.ResultID, result.EnrolmentID, result.FinishTime,
            result.NetTime, result.FinishingPosition, result.OverallPlacement,
            result.CategoryPlacement, result.Status, result.IsPublished, result.Notes));
    }
}
