using System;
using System.Collections.Generic;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;

public class UserResponseDto
{
    public Guid id { get; set; }
    public string email { get; set; } = string.Empty;
    public UserStatus status { get; set; }
    public DateTimeOffset createdAt { get; set; }
    public List<string> roles { get; set; } = new();
    public ProfileResponseDto? profile { get; set; }
}

public class ProfileResponseDto
{
    public string fullName { get; set; } = string.Empty;
    public string phone { get; set; } = string.Empty;
    public string? avatarUrl { get; set; }
    public DateTimeOffset? dateOfBirth { get; set; }
}
