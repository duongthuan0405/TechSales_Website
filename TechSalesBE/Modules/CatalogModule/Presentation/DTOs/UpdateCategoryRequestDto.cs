using System.Text.Json.Serialization;

namespace CatalogModule.Presentation.DTOs;

public sealed record UpdateCategoryRequestDto(
    [property: JsonRequired] string Name);
