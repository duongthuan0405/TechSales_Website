using System;
using TechSalesManagement.Domain.Common;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Domain.Entities;

public class Article
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }

    public string title { get; set; } = string.Empty;
    public string slug { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
    public string? thumbnailUrl { get; set; }
    public ArticleStatus status { get; set; } = ArticleStatus.DRAFT;
    public Guid authorId { get; set; }

    public Article(string title, string content, Guid authorId)
    {
        this.title = title;
        this.content = content;
        this.authorId = authorId;
    }

    public Article() { }

    public void Publish()
    {
        this.status = ArticleStatus.PUBLISHED;
        this.updatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        this.status = ArticleStatus.ARCHIVED;
        this.updatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateContent(string title, string content, string slug, string? thumbnailUrl)
    {
        this.title = title;
        this.content = content;
        this.slug = slug;
        this.thumbnailUrl = thumbnailUrl;
        this.updatedAt = DateTimeOffset.UtcNow;
    }
}
