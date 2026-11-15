using System;
using System.ComponentModel.DataAnnotations;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

public class OrderStaffCancelRequestDto
{
    [Required]
    public string reason { get; set; } = string.Empty;
}
