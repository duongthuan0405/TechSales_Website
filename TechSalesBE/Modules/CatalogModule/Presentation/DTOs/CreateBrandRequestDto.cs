using System.Text.Json.Serialization;

namespace CatalogModule.Presentation.DTOs;

public sealed record CreateBrandRequestDto(
    [property: JsonRequired] string Name);
