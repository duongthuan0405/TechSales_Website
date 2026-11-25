using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

[Table("system_settings")]
public class SystemSettingDbModel
{
    [Key, MaxLength(100)]
    public string key { get; set; } = string.Empty;
    
    [Required]
    public string value { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? description { get; set; }
    
    public DateTimeOffset updated_at { get; set; }
}
