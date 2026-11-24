using System.Text.RegularExpressions;
using CatalogModule.Application.Exceptions;
using CatalogModule.Application.Ports;
using CatalogModule.Infrastructure.Configurations;
using CommonModule.BusinessExceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace CatalogModule.Infrastructure.Services;

internal sealed class LocalProductImageStorage : IProductImageStorage
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalProductImageStorage> _logger;

    public LocalProductImageStorage(IWebHostEnvironment environment, ILogger<LocalProductImageStorage> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveAsync(Stream content, string contentType, long length,
        CancellationToken cancellationToken)
    {
        if (length <= 0 || length > ProductImageStorageLimits.MaximumFileSizeBytes)
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST,
                BusinessExceptionMessage.ImageSizeInvalid(ProductImageStorageLimits.MaximumFileSizeBytes));
        }

        using MemoryStream buffer = new MemoryStream();
        byte[] chunk = new byte[8192];
        int bytesRead;
        while ((bytesRead = await content.ReadAsync(chunk.AsMemory(), cancellationToken)) > 0)
        {
            if (buffer.Length + bytesRead > ProductImageStorageLimits.MaximumFileSizeBytes)
            {
                throw new BusinessException(BusinessExceptionCode.BAD_REQUEST,
                    BusinessExceptionMessage.ImageSizeInvalid(ProductImageStorageLimits.MaximumFileSizeBytes));
            }

            await buffer.WriteAsync(chunk.AsMemory(0, bytesRead), cancellationToken);
        }

        if (buffer.Length != length)
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST,
                BusinessExceptionMessage.ImageSizeInvalid(ProductImageStorageLimits.MaximumFileSizeBytes));
        }

        byte[] data = buffer.ToArray();
        string extension = contentType.ToLowerInvariant() switch
        {
            "image/png" when data.Length >= 8 && data.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) => ".png",
            "image/jpeg" when data.Length >= 3 && data[0] == 255 && data[1] == 216 && data[2] == 255 => ".jpg",
            "image/webp" when data.Length >= 12 && data.AsSpan(0, 4).SequenceEqual("RIFF"u8) && data.AsSpan(8, 4).SequenceEqual("WEBP"u8) => ".webp",
            _ => throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.ImageTypeUnsupported)
        };

        string directory = GetDirectory();
        Directory.CreateDirectory(directory);
        string fileName = Guid.NewGuid().ToString("N") + extension;
        string path = Path.Combine(directory, fileName);
        try
        {
            await using FileStream file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await file.WriteAsync(data.AsMemory(), cancellationToken);
        }
        catch
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception cleanupException)
            {
                _logger.LogError(cleanupException, "Cannot remove incomplete product image {FileName}.", fileName);
            }

            throw;
        }

        return "/product-images/" + fileName;
    }

    public Task DeleteAsync(string imageUrl, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Regex.IsMatch(imageUrl, "^/product-images/[a-f0-9]{32}\\.(png|jpg|webp)$",
            RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
        {
            throw new InvalidOperationException("Invalid product image storage path.");
        }

        File.Delete(Path.Combine(GetDirectory(), Path.GetFileName(imageUrl)));
        return Task.CompletedTask;
    }

    private string GetDirectory()
    {
        string webRoot = _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        return Path.Combine(webRoot, "product-images");
    }
}
