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
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => HttpContext.Session.GetInt32("UserID")!.Value;

    [HttpGet("events/{eventId}/categories")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId)) return NotFound("Event not found.");

        var categories = await _db.Categories.Where(c => c.EventID == eventId)
            .Select(c => new CategoryResponseDto(c.CategoryID, c.EventID, c.Name, c.Description, c.CategoryType,
                c.MinAge, c.MaxAge, c.Distance, c.EntryFee, c.Capacity)).ToListAsync();

        return Ok(categories);
    }

    [HttpPost("events/{eventId}/categories")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> Create(int eventId, CategoryCreateDto dto)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound("Event not found.");

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserId);
        if (organiser == null || ev.OrganiserID != organiser.OrganiserID) return Forbid();

        var category = new Category
        {
            EventID = eventId,
            Name = dto.Name,
            Description = dto.Description,
            CategoryType = dto.CategoryType,
            MinAge = dto.MinAge,
            MaxAge = dto.MaxAge,
            Distance = dto.Distance,
            EntryFee = dto.EntryFee,
            Capacity = dto.Capacity
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetForEvent), new { eventId },
            new CategoryResponseDto(category.CategoryID, category.EventID, category.Name,
                category.Description, category.CategoryType, category.MinAge,
                category.MaxAge, category.Distance, category.EntryFee, category.Capacity));
    }

    [HttpPut("categories/{id}")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, CategoryUpdateDto dto)
    {
        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null) return NotFound();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserId);
        if (organiser == null || category.Event.OrganiserID != organiser.OrganiserID) return Forbid();

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.CategoryType = dto.CategoryType;
        category.MinAge = dto.MinAge;
        category.MaxAge = dto.MaxAge;
        category.Distance = dto.Distance;
        category.EntryFee = dto.EntryFee;
        category.Capacity = dto.Capacity;

        await _db.SaveChangesAsync();
        return Ok(new CategoryResponseDto(category.CategoryID, category.EventID, category.Name,
            category.Description, category.CategoryType, category.MinAge,
            category.MaxAge, category.Distance, category.EntryFee, category.Capacity));
    }

    [HttpDelete("categories/{id}")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null) return NotFound();

        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.UserID == CurrentUserId);
        if (organiser == null || category.Event.OrganiserID != organiser.OrganiserID) return Forbid();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
