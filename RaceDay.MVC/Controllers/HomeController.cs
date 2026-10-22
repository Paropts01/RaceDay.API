using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Attributes;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[SessionRequired]
public class HomeController : Controller
{
    private readonly ApiClient _api;

    public HomeController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(
        string? type, DateTime? date, string? location)
    {
        var url = "api/events";
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(type))
            query.Add($"type={Uri.EscapeDataString(type)}");

        if (date.HasValue)
            query.Add($"date={date.Value:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(location))
            query.Add($"location={Uri.EscapeDataString(location)}");

        if (query.Any())
            url += "?" + string.Join("&", query);

        var events = await _api.GetJsonAsync<List<EventResponseDto>>(url)
                     ?? new List<EventResponseDto>();

        ViewBag.Type = type;
        ViewBag.Date = date?.ToString("yyyy-MM-dd");
        ViewBag.Location = location;

        return View(events);
    }
}
