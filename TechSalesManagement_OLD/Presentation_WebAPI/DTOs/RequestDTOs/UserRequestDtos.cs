using Microsoft.AspNetCore.Http;
using System;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

public class UpdateProfileRequestDto
{
    public string? fullName { get; set; }
    public string? phone { get; set; }
    public string? avatarUrl { get; set; }
    public DateTime? dateOfBirth { get; set; }
    public IFormFile? avatarFile { get; set; }
}
