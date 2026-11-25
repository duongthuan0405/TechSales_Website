using Microsoft.AspNetCore.Http;
using System;

namespace TechSalesManagement.Application.Services.Params;

public class UpdateProfileParams
{
    public required Guid UserId { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public IFormFile? AvatarFile { get; set; }
}
