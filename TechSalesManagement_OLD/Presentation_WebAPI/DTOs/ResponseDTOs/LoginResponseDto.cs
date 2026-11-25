namespace TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;

public class LoginResponseDto
{
    public string token { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public List<string> roles { get; set; } = new();
}
