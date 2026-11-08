
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Attributes;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/files")]
[SessionAuthorize]
public class FilesController : ControllerBase
{
    private readonly AzureBlobStorageService _blob;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        AzureBlobStorageService blob,
        ILogger<FilesController> logger)
    {
        _blob = blob;
        _logger = logger;
    }

    [HttpPost("event-banner")]
    [RoleAuthorize("Organiser")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadEventBanner(IFormFile file)
    {
        try
        {
            var url = await _blob.UploadAsync(file, "event-banners");

            _logger.LogInformation(
                "Event banner uploaded successfully: {BlobUrl}", url);

            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event banner upload failed.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("profile-picture")]
    [RoleAuthorize("Participant")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        try
        {
            var url = await _blob.UploadAsync(file, "profile-pictures");

            _logger.LogInformation(
                "Profile picture uploaded successfully: {BlobUrl}", url);

            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Profile picture upload failed.");
            return BadRequest(ex.Message);
        }
    }
}

