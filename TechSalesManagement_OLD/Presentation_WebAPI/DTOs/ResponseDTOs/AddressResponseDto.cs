using System;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;

public class AddressResponseDto
{
    public Guid id { get; set; }
    public string province { get; set; } = string.Empty;
    public string ward { get; set; } = string.Empty;
    public string detail { get; set; } = string.Empty;
    public bool isDefault { get; set; }
}
