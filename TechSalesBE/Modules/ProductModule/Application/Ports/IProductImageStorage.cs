namespace ProductModule.Application.Ports;

internal interface IProductImageStorage
{
    Task<string> SaveAsync(Stream content, string contentType, long length, CancellationToken cancellationToken);
    Task DeleteAsync(string imageUrl, CancellationToken cancellationToken);
}
