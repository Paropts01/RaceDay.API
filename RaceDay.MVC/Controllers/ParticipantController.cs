using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Attributes;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[SessionRequired]
[RoleRequired("Participant")]
public class ParticipantController : Controller
{
    private readonly ApiClient _api;

    public ParticipantController(ApiClient api) => _api = api;

    public async Task<IActionResult> MyEvents()
    {
        var enrolments =
            await _api.GetJsonAsync<List<MyEnrolmentDto>>(
                "api/participants/enrolments")
            ?? new List<MyEnrolmentDto>();

        return View(enrolments);
    }

    public async Task<IActionResult> MyResults()
    {
        var results =
            await _api.GetJsonAsync<List<MyResultDto>>(
                "api/participants/results")
            ?? new List<MyResultDto>();

        return View(results);
    }

    public async Task<IActionResult> Profile()
    {
        var profile =
            await _api.GetJsonAsync<ProfileResponseDto>("api/profile");

        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(
        string firstName, string lastName, string email,
        string? phoneNumber, DateTime dateOfBirth)
    {
        var response = await _api.PutJsonAsync("api/profile", new
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth
        });

        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
            response.IsSuccessStatusCode
                ? "Profile updated."
                : await response.Content.ReadAsStringAsync();

        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    public async Task<IActionResult> UploadProfilePicture(IFormFile picture)
    {
        var upload = await _api.UploadAsync(
            "api/files/profile-picture", picture);

        if (!upload.IsSuccessStatusCode)
        {
            TempData["Error"] = await upload.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Profile));
        }

        var uploadResult =
            await upload.Content.ReadFromJsonAsync<UploadResponse>();

        var update = await _api.PutJsonAsync("api/profile/picture", new
        {
            ProfilePictureURL = uploadResult!.Url
        });

        TempData[update.IsSuccessStatusCode ? "Success" : "Error"] =
            update.IsSuccessStatusCode
                ? "Profile picture updated."
                : await update.Content.ReadAsStringAsync();

        return RedirectToAction(nameof(Profile));
    }
}
