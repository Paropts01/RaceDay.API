using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests;

public class EnrolmentsControllerTests
{
    private RaceDayDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RaceDayDbContext(options);
    }

    [Fact]
    public async Task Enrol_AsParticipant_ReturnsCreated()
    {
        var db = GetDbContext();
        // Seed organiser, event, category, participant
        var orgUser = new User { Username = "org", Email = "org@test.com", PasswordHash = "x", PasswordSalt = "y", FirstName = "O", LastName = "G", DateOfBirth = DateTime.Today, Role = "Organiser" };
        var partUser = new User { Username = "part", Email = "part@test.com", PasswordHash = "x", PasswordSalt = "y", FirstName = "P", LastName = "T", DateOfBirth = DateTime.Today, Role = "Participant" };
        db.Users.AddRange(orgUser, partUser);
        await db.SaveChangesAsync();
        var org = new Organiser { UserID = orgUser.UserID };
        var part = new Participant { UserID = partUser.UserID };
        db.Organisers.Add(org);
        db.Participants.Add(part);
        await db.SaveChangesAsync();
        var ev = new Event { OrganiserID = org.OrganiserID, Name = "Run", EventDate = DateTime.Today.AddDays(30), Location = "CT", Distance = 10, EventType = "Run" };
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        var cat = new Category { EventID = ev.EventID, Name = "Open", EntryFee = 100 };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();

        var controller = new EnrolmentsController(db);
        DefaultHttpContext httpContext = new DefaultHttpContext { Session = new TestSession() };
        httpContext.Session.SetInt32("UserID", partUser.UserID);
        httpContext.Session.SetString("Role", "Participant");
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await controller.Enrol(ev.EventID, new EnrolDto(cat.CategoryID));
        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Single(db.EventEnrolments);
    }

    [Fact]
    public async Task Enrol_Duplicate_ReturnsBadRequest()
    {
        var db = GetDbContext();
        var orgUser = new User { Username = "org", Email = "org@test.com", PasswordHash = "x", PasswordSalt = "y", FirstName = "O", LastName = "G", DateOfBirth = DateTime.Today, Role = "Organiser" };
        var partUser = new User { Username = "part", Email = "part@test.com", PasswordHash = "x", PasswordSalt = "y", FirstName = "P", LastName = "T", DateOfBirth = DateTime.Today, Role = "Participant" };
        db.Users.AddRange(orgUser, partUser);
        await db.SaveChangesAsync();
        var org = new Organiser { UserID = orgUser.UserID };
        var part = new Participant { UserID = partUser.UserID };
        db.Organisers.Add(org);
        db.Participants.Add(part);
        await db.SaveChangesAsync();
        var ev = new Event { OrganiserID = org.OrganiserID, Name = "Run", EventDate = DateTime.Today.AddDays(30), Location = "CT", Distance = 10, EventType = "Run" };
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        var cat = new Category { EventID = ev.EventID, Name = "Open", EntryFee = 100 };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();
        db.EventEnrolments.Add(new EventEnrolment { ParticipantID = part.ParticipantID, EventID = ev.EventID, CategoryID = cat.CategoryID });
        await db.SaveChangesAsync();

        var controller = new EnrolmentsController(db);
        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        httpContext.Session.SetInt32("UserID", partUser.UserID);
        httpContext.Session.SetString("Role", "Participant");
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await controller.Enrol(ev.EventID, new EnrolDto(cat.CategoryID));
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
