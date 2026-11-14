
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

    public OrganiserController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Dashboard()
    {
        var profile = await _api.GetJsonAsync<ProfileResponseDto>(
            "api/profile");

        var events = await _api.GetJsonAsync<List<EventResponseDto>>(
            "api/events")
            ?? new List<EventResponseDto>();

        var myEvents = events
            .Where(e => e.OrganiserID == profile?.Organiser?.OrganiserID)
            .ToList();

        ViewBag.OrganiserName =
            profile?.Organiser?.OrganisationName ?? profile?.FirstName;

        return View(myEvents);
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        var model = new EventCreateDto(
            "",
            null,
            DateTime.Today,
            "",
            0m,
            "Run",
            null,
            null,
            null
        );

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(
        EventCreateDto dto,
        IFormFile? banner)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        // Upload the selected banner through the API.
        if (banner != null && banner.Length > 0)
        {
            var upload = await _api.UploadAsync(
                "api/files/event-banner",
                banner);

            if (!upload.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    await upload.Content.ReadAsStringAsync();

                return View(dto);
            }

            var uploadResult =
                await upload.Content.ReadFromJsonAsync<UploadResponse>();

            if (uploadResult == null ||
                string.IsNullOrWhiteSpace(uploadResult.Url))
            {
                ViewBag.Error =
                    "The API did not return a valid image URL.";

                return View(dto);
            }

            // Store the Azure image URL in the event DTO.
            dto = dto with
            {
                BannerImageURL = uploadResult.Url
            };
        }

        // Send the event information and Azure image URL to the API.
        var response = await _api.PostJsonAsync(
            "api/events",
            dto);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error =
                await response.Content.ReadAsStringAsync();

            return View(dto);
        }

        TempData["Success"] = "Event created successfully.";

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
        int eventId,
        CategoryCreateDto dto)
    {
        var response = await _api.PostJsonAsync(
            $"api/events/{eventId}/categories",
            dto);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] =
                await response.Content.ReadAsStringAsync();
        }

        return RedirectToAction(
            nameof(Categories),
            new { eventId });
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
        int eventId,
        ResultCreateDto dto)
    {
        var response = await _api.PostJsonAsync(
            $"api/events/{eventId}/results",
            dto);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] =
                await response.Content.ReadAsStringAsync();
        }

        return RedirectToAction(
            nameof(Results),
            new { eventId });
    }
}
