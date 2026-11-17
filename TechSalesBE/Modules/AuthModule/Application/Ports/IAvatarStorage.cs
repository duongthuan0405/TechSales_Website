namespace AuthModule.Application.Ports;
internal interface IAvatarStorage
{
    Task<string> SaveAsync(Stream content, string contentType, long length, CancellationToken cancellationToken);
}
