namespace CatalogModule.Application.Services;

internal sealed record ProductImageUpload(Stream Content, string ContentType, long Length);
