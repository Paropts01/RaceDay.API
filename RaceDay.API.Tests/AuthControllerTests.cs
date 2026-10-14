using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Tests;
using Xunit;

namespace RaceDay.API.Tests;

public class AuthControllerTests
{
    private RaceDayDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RaceDayDbContext(options);
    }

    [Fact]
    public async Task Register_ValidOrganiser_ReturnsCreated()
    {
        var db = GetDbContext();
        var controller = new AuthController(db);
        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var dto = new RegisterDto("org1", "org1@test.com", "Password123!", "John", "Doe",
            new DateTime(1985, 1, 1), "+27821234567", "Organiser");

        var result = await controller.Register(dto);

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Single(db.Users);
        Assert.Single(db.Organisers);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var db = GetDbContext();
        db.Users.Add(new API.Models.User { Username = "u1", Email = "dup@test.com", PasswordHash = "x", PasswordSalt = "y", FirstName = "A", LastName = "B", DateOfBirth = DateTime.Today, Role = "Participant" });
        await db.SaveChangesAsync();

        var controller = new AuthController(db);
        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var dto = new RegisterDto("u2", "dup@test.com", "Password123!", "C", "D",
            new DateTime(1990, 1, 1), null, "Participant");

        var result = await controller.Register(dto);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var db = GetDbContext();
        var (hash, salt) = RaceDay.API.Services.PasswordHasher.CreateHash("Password123!");
        db.Users.Add(new API.Models.User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.Today,
            Role = "Participant"
        });
        await db.SaveChangesAsync();

        var controller = new AuthController(db);
        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await controller.Login(new LoginDto("testuser", "Password123!"));
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var db = GetDbContext();
        var (hash, salt) = RaceDay.API.Services.PasswordHasher.CreateHash("CorrectPassword");
        db.Users.Add(new API.Models.User
        {
            Username = "user",
            Email = "u@test.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            FirstName = "A",
            LastName = "B",
            DateOfBirth = DateTime.Today,
            Role = "Participant"
        });
        await db.SaveChangesAsync();

        var controller = new AuthController(db);
        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await controller.Login(new LoginDto("user", "WrongPassword"));
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
