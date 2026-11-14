

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api)
    {
        _api = api;
    }

    // Display Login page
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // Process Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string usernameOrEmail,
        string password)
    {
        var response = await _api.PostJsonAsync("api/auth/login", new
        {
            UsernameOrEmail = usernameOrEmail,
            Password = password
        });

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = await response.Content.ReadAsStringAsync();
            return View();
        }

        var user = await response.Content.ReadFromJsonAsync<UserResponseDto>();

        if (user == null)
        {
            ViewBag.Error = "Unable to retrieve your account details. Please try again.";
            return View();
        }

        // Store logged-in user's details in the session
        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("UserName", user.FirstName);

        return RedirectToAction("Index", "Home");
    }

    // Display Register page
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // Process Registration
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        string username,
        string email,
        string password,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string? phoneNumber,
        string role)
    {
        var response = await _api.PostJsonAsync("api/auth/register", new
        {
            Username = username,
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            PhoneNumber = phoneNumber,
            Role = role
        });

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = await response.Content.ReadAsStringAsync();
            return View();
        }

        var user = await response.Content.ReadFromJsonAsync<UserResponseDto>();

        if (user == null)
        {
            ViewBag.Error = "Registration succeeded, but account details could not be retrieved.";
            return View();
        }

        // Store newly registered user's details in the session
        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("UserName", user.FirstName);

        return RedirectToAction("Index", "Home");
    }

    // Log out the current user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        // Clear the user's session
        HttpContext.Session.Clear();

        // Redirect to the Login page
        return RedirectToAction("Login", "Account");
    }
}
