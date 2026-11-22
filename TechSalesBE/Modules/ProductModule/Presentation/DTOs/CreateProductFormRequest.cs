using Microsoft.AspNetCore.Http;

namespace ProductModule.Presentation.DTOs;

public sealed class CreateProductFormRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long? Price { get; set; }
    public Guid BrandId { get; set; }
    public Guid CategoryId { get; set; }
    public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    public int? PrimaryImageIndex { get; set; }
}
