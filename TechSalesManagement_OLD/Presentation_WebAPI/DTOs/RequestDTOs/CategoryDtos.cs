using System.ComponentModel.DataAnnotations;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

public class CreateCategoryRequestDto
{
    [Required]
    [MaxLength(100)]
    public string name { get; set; } = string.Empty;
}

public class DeleteCategoryRequestDto
{
    [Required]
    public Guid replacementCategoryId { get; set; }
}
