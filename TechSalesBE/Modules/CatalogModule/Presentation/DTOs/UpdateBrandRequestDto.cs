using System.Text.Json.Serialization;

namespace CatalogModule.Presentation.DTOs;

public sealed record UpdateBrandRequestDto(
    [property: JsonRequired] string Name);
