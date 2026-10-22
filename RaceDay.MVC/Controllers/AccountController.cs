using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string usernameOrEmail, string password)
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

        HttpContext.Session.SetInt32("UserID", user!.UserID);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("UserName", user.FirstName);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(
        string username, string email, string password,
        string firstName, string lastName, DateTime dateOfBirth,
        string? phoneNumber, string role)
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

        HttpContext.Session.SetInt32("UserID", user!.UserID);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("UserName", user.FirstName);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _api.PostJsonAsync("api/auth/logout", new { });
        HttpContext.Session.Clear();

        return RedirectToAction("Login");
    }
}
