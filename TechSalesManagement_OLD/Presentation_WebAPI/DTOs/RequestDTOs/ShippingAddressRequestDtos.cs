namespace TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

public class CreateAddressRequestDto
{
    public string province { get; set; } = string.Empty;
    public string ward { get; set; } = string.Empty;
    public string detail { get; set; } = string.Empty;
}

public class UpdateAddressRequestDto
{
    public string province { get; set; } = string.Empty;
    public string ward { get; set; } = string.Empty;
    public string detail { get; set; } = string.Empty;
}
