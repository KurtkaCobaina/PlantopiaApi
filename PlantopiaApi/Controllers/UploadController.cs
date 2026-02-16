// Controllers/UploadController.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace PlantopiaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public UploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest("No image file provided.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
        if (!allowedTypes.Contains(image.ContentType.ToLowerInvariant()))
            return BadRequest("Only JPEG and PNG images are allowed.");

        try
        {
            var wwwrootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(wwwrootPath))
            {
                wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var uploadDir = Path.Combine(wwwrootPath, "uploads");
            Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploadDir, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Upload error: {ex}");
            return StatusCode(500, "Failed to upload image.");
        }
    }
}