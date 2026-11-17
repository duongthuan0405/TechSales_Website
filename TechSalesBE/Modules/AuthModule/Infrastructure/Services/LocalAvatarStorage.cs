using AuthModule.Domain.Constants;
using AuthModule.Application.Ports;
using CommonModule.BusinessExceptions;
using Microsoft.AspNetCore.Hosting;

namespace AuthModule.Infrastructure.Services;
internal sealed class LocalAvatarStorage : IAvatarStorage
{
    private readonly IWebHostEnvironment _environment;

    public LocalAvatarStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(Stream content, string contentType, long length, CancellationToken cancellationToken)
    {
        if (length is <= 0 or > AuthLimits.AvatarMaxSizeBytes)
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, $"Avatar must be at most {AuthLimits.AvatarMaxSizeMegabytes} MB.");
        }

        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int read;
        while ((read = await content.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > AuthLimits.AvatarMaxSizeBytes)
            {
                throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, $"Avatar must be at most {AuthLimits.AvatarMaxSizeMegabytes} MB.");
            }

            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }

        var data = buffer.ToArray();
        var extension = contentType switch
        {
            "image/png" when data.Length >= 8 && data.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) => ".png",
            "image/jpeg" when data.Length >= 3 && data[0] == 255 && data[1] == 216 && data[2] == 255 => ".jpg",
            "image/webp" when data.Length >= 12 && data.AsSpan(0, 4).SequenceEqual("RIFF"u8) && data.AsSpan(8, 4).SequenceEqual("WEBP"u8) => ".webp",
            _ => throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, "Only PNG, JPEG and WebP avatars are supported.")};
        var directory = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "avatars");
        Directory.CreateDirectory(directory);
        var name = Guid.NewGuid().ToString("N") + extension;
        await File.WriteAllBytesAsync(Path.Combine(directory, name), data, cancellationToken);
        return "/avatars/" + name;
    }
}
