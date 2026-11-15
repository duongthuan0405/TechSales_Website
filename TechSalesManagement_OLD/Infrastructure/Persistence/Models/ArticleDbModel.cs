using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

[Table("articles")]
public class ArticleDbModel
{
    [Key]
    public Guid id { get; set; }
    
    [Required, MaxLength(255)]
    public string title { get; set; } = string.Empty;
    
    [Required, MaxLength(255)]
    public string slug { get; set; } = string.Empty;
    
    [Required]
    public string content { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? thumbnail_url { get; set; }
    
    public ArticleStatus status { get; set; }
    
    public Guid author_id { get; set; }
    
    public DateTimeOffset created_at { get; set; }
    public DateTimeOffset? updated_at { get; set; }

    // Navigation
    [ForeignKey("author_id")]
    public UserDbModel author { get; set; } = null!;
}
