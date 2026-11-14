using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Attributes;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[SessionRequired]
public class EventsController : Controller
{
    private readonly ApiClient _api;

    public EventsController(ApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ev = await _api.GetJsonAsync<EventResponseDto>(
            $"api/events/{id}");

        if (ev == null)
        {
            return NotFound();
        }

        var categories =
            await _api.GetJsonAsync<List<CategoryResponseDto>>(
                $"api/events/{id}/categories")
            ?? new List<CategoryResponseDto>();

        ViewBag.Categories = categories;

        return View(ev);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleRequired("Organiser")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadBanner(
        int eventId,
        IFormFile banner)
    {
        if (banner == null || banner.Length == 0)
        {
            TempData["Error"] = "Please select an image.";

            return RedirectToAction(
                nameof(Details),
                new { id = eventId });
        }

        var response = await _api.UploadAsync(
            $"api/events/{eventId}/banner",
            banner,
            "file");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] =
                await response.Content.ReadAsStringAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = eventId });
        }

        TempData["Success"] =
            "Event banner uploaded successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = eventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleRequired("Participant")]
    public async Task<IActionResult> Enrol(
        int eventId,
        int categoryId)
    {
        var response = await _api.PostJsonAsync(
            $"api/events/{eventId}/enrol",
            new { CategoryId = categoryId });

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] =
                await response.Content.ReadAsStringAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = eventId });
        }

        TempData["Success"] =
            "You have enrolled successfully.";

        return RedirectToAction(
            "MyEvents",
            "Participant");
    }
}
