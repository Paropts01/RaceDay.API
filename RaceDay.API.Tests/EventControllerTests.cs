
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.API.Tests;

public class EventsControllerTests
{
    private RaceDayDbContext GetDbContext()
    {
        var options =
            new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new RaceDayDbContext(options);
    }

    private AzureBlobStorageService GetBlobStorageService()
    {
        // Create configuration settings for the test.
        var settings = new Dictionary<string, string?>
        {
            ["AzureBlob:ConnectionString"] =
                "UseDevelopmentStorage=true",

            ["AzureBlob:ContainerName"] = "raceday"
        };

        IConfiguration configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

        // Create the BlobServiceClient.
        // These tests do not upload files, so an Azure Storage
        // emulator does not need to be running.
        var blobServiceClient = new BlobServiceClient(
            "UseDevelopmentStorage=true");

        return new AzureBlobStorageService(
            blobServiceClient,
            configuration);
    }

    private EventsController CreateController(
        RaceDayDbContext db,
        int userId,
        string role)
    {
        var controller = new EventsController(
            db,
            GetBlobStorageService());

        var httpContext = new DefaultHttpContext
        {
            Session = new TestSession()
        };

        httpContext.Session.SetInt32("UserID", userId);
        httpContext.Session.SetString("Role", role);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_ReturnsCreated()
    {
        // Arrange
        using var db = GetDbContext();

        var user = new User
        {
            Username = "org",
            Email = "org@test.com",
            PasswordHash = "x",
            PasswordSalt = "y",
            FirstName = "O",
            LastName = "G",
            DateOfBirth = DateTime.Today,
            Role = "Organiser"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.Organisers.Add(new Organiser
        {
            UserID = user.UserID
        });

        await db.SaveChangesAsync();

        var controller = CreateController(
            db,
            user.UserID,
            "Organiser");

        var dto = new EventCreateDto(
            "Test Run",
            "Desc",
            DateTime.Today.AddDays(30),
            "Cape Town",
            10,
            "Run",
            null,
            100,
            null);

        // Act
        var result = await controller.Create(dto);

        // Assert
        var createdResult =
            Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(1, await db.Events.CountAsync());

        var createdEvent = await db.Events.FirstAsync();

        Assert.Equal("Test Run", createdEvent.Name);
        Assert.Equal(user.UserID, createdEvent.OrganiserID);
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_ReturnsForbid()
    {
        // Arrange
        using var db = GetDbContext();

        var user = new User
        {
            Username = "part",
            Email = "part@test.com",
            PasswordHash = "x",
            PasswordSalt = "y",
            FirstName = "P",
            LastName = "T",
            DateOfBirth = DateTime.Today,
            Role = "Participant"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.Participants.Add(new Participant
        {
            UserID = user.UserID
        });

        await db.SaveChangesAsync();

        var controller = CreateController(
            db,
            user.UserID,
            "Participant");

        var dto = new EventCreateDto(
            "Test Run",
            "Desc",
            DateTime.Today.AddDays(30),
            "Cape Town",
            10,
            "Run",
            null,
            100,
            null);

        // Act
        var result = await controller.Create(dto);

        // Assert
        Assert.IsType<ForbidResult>(result);

        Assert.Equal(0, await db.Events.CountAsync());
    }
}

