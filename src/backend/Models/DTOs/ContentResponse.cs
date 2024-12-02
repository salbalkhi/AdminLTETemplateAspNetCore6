namespace Tadawi.Models.DTOs;

public class ContentResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? FileUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public UserPreviewResponse Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
