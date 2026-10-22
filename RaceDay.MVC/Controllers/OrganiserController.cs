using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Attributes;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[SessionRequired]
[RoleRequired("Organiser")]
public class OrganiserController : Controller
{
    private readonly ApiClient _api;

    public OrganiserController(ApiClient api) => _api = api;

    public async Task<IActionResult> Dashboard()
    {
        var profile = await _api.GetJsonAsync<ProfileResponseDto>("api/profile");

        var events = await _api.GetJsonAsync<List<EventResponseDto>>("api/events")
                     ?? new List<EventResponseDto>();

        var myEvents = events
            .Where(e => e.OrganiserID == profile!.Organiser!.OrganiserID)
            .ToList();

        ViewBag.OrganiserName =
            profile?.Organiser?.OrganisationName ?? profile?.FirstName;

        return View(myEvents);
    }

    [HttpGet]
    public IActionResult CreateEvent() => View();

    [HttpPost]
    public async Task<IActionResult> CreateEvent(
        EventCreateDto dto, IFormFile? banner)
    {
        if (banner != null && banner.Length > 0)
        {
            var upload = await _api.UploadAsync(
                "api/files/event-banner", banner);

            if (!upload.IsSuccessStatusCode)
            {
                ViewBag.Error = await upload.Content.ReadAsStringAsync();
                return View(dto);
            }

            var uploadResult =
                await upload.Content.ReadFromJsonAsync<UploadResponse>();

            dto = dto with { BannerImageURL = uploadResult!.Url };
        }

        var response = await _api.PostJsonAsync("api/events", dto);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = await response.Content.ReadAsStringAsync();
            return View(dto);
        }

        return RedirectToAction(nameof(Dashboard));
    }

    public async Task<IActionResult> Categories(int eventId)
    {
        var categories =
            await _api.GetJsonAsync<List<CategoryResponseDto>>(
                $"api/events/{eventId}/categories")
            ?? new List<CategoryResponseDto>();

        ViewBag.EventId = eventId;
        return View(categories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        int eventId, CategoryCreateDto dto)
    {
        var response = await _api.PostJsonAsync(
            $"api/events/{eventId}/categories", dto);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
        }

        return RedirectToAction(nameof(Categories), new { eventId });
    }

    public async Task<IActionResult> Enrolments(int eventId)
    {
        var enrolments =
            await _api.GetJsonAsync<List<EventEnrolmentListItemDto>>(
                $"api/events/{eventId}/enrolments")
            ?? new List<EventEnrolmentListItemDto>();

        ViewBag.EventId = eventId;
        return View(enrolments);
    }

    public async Task<IActionResult> Results(int eventId)
    {
        var results =
            await _api.GetJsonAsync<List<ResultListItemDto>>(
                $"api/events/{eventId}/results")
            ?? new List<ResultListItemDto>();

        ViewBag.EventId = eventId;
        return View(results);
    }

    [HttpPost]
    public async Task<IActionResult> CreateResult(
        int eventId, ResultCreateDto dto)
    {
        var response = await _api.PostJsonAsync(
            $"api/events/{eventId}/results", dto);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
        }

        return RedirectToAction(nameof(Results), new { eventId });
    }
}

public record EventCreateDto(
    string Name, string? Description, DateTime EventDate,
    string Location, decimal Distance, string EventType,
    string? BannerImageURL, int? MaxParticipants,
    DateTime? RegistrationDeadline);

public record CategoryCreateDto(
    string Name, string? Description, string? CategoryType,
    int? MinAge, int? MaxAge, decimal? Distance,
    decimal? EntryFee, int? Capacity);

public record ResultCreateDto(
    int EnrolmentId, TimeSpan? FinishTime, TimeSpan? NetTime,
    int? FinishingPosition, int? OverallPlacement,
    int? CategoryPlacement, string Status, string? Notes);
