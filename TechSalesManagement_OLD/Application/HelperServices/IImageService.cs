using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TechSalesManagement.Application.HelperServices;

public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile file);
    Task<bool> DeleteImageAsync(string publicId);
}
