using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Attributes;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[SessionRequired]
public class EventsController : Controller
{
    private readonly ApiClient _api;

    public EventsController(ApiClient api) => _api = api;

    public async Task<IActionResult> Details(int id)
    {
        var ev = await _api.GetJsonAsync<EventResponseDto>($"api/events/{id}");

        if (ev == null)
            return NotFound();

        var categories =
            await _api.GetJsonAsync<List<CategoryResponseDto>>(
                $"api/events/{id}/categories")
            ?? new List<CategoryResponseDto>();

        ViewBag.Categories = categories;
        return View(ev);
    }

    [HttpPost]
    [RoleRequired("Participant")]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        var response = await _api.PostJsonAsync(
            $"api/events/{eventId}/enrol",
            new { CategoryId = categoryId });

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        TempData["Success"] = "You have enrolled successfully.";
        return RedirectToAction("MyEvents", "Participant");
    }
}
