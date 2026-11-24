using Microsoft.AspNetCore.Http;

namespace CatalogModule.Presentation.DTOs;

public sealed class UpdateProductImagesRequest
{
    public List<Guid> RetainedImageIds { get; set; } = new List<Guid>();
    public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    public int? PrimaryImageIndex { get; set; }
}
