using System.Text.Json.Serialization;

namespace CatalogModule.Presentation.DTOs;

public sealed record CreateCategoryRequestDto(
    [property: JsonRequired] string Name);
