using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.HelperServices;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IImageService _imageService;

    public UploadController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        var url = await _imageService.UploadImageAsync(file);
        return Ok(new { url });
    }
}
