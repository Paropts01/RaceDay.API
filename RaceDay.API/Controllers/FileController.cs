using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Attributes;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/files")]
[SessionAuthorize]
public class FilesController : ControllerBase
{
    private readonly BlobStorageService _blob;

    public FilesController(BlobStorageService blob)
    {
        _blob = blob;
    }

    [HttpPost("event-banner")]
    [RoleAuthorize("Organiser")]
    public async Task<IActionResult> UploadEventBanner(IFormFile file)
    {
        try
        {
            var url = await _blob.UploadAsync(file, "event-banners");
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("profile-picture")]
    [RoleAuthorize("Participant")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        try
        {
            var url = await _blob.UploadAsync(file, "profile-pictures");
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
