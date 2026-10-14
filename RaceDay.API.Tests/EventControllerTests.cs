using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Tests;
using Xunit;

namespace RaceDay.API.Tests;

public class EventsControllerTests
{
    private RaceDayDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RaceDayDbContext(options);
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_ReturnsCreated()
    {
        var db = GetDbContext();
        var user = new User { Username = "org", Email = "org@test.com", PasswordHash = "x", PasswordSalt = "y", FirstName = "O", LastName = "G", DateOfBirth = DateTime.Today, Role = "Organiser" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.Organisers.Add(new Organiser { UserID = user.UserID });
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        httpContext.Session.SetInt32("UserID", user.UserID);
        httpContext.Session.SetString("Role", "Organiser");
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var dto = new EventCreateDto("Test Run", "Desc", DateTime.Today.AddDays(30), "Cape Town", 10, "Run", null, 100, null);
        var result = await controller.Create(dto);
        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_ReturnsForbid()
    {
        var db = GetDbContext();
        var user = new User { Username = "part", Email = "part@test.com", PasswordHash = "x", PasswordSalt = "y", FirstName = "P", LastName = "T", DateOfBirth = DateTime.Today, Role = "Participant" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.Participants.Add(new Participant { UserID = user.UserID });
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        httpContext.Session.SetInt32("UserID", user.UserID);
        httpContext.Session.SetString("Role", "Participant");
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var dto = new EventCreateDto("Test Run", "Desc", DateTime.Today.AddDays(30), "Cape Town", 10, "Run", null, 100, null);
        var result = await controller.Create(dto);
        Assert.IsType<ForbidResult>(result);
    }
}
