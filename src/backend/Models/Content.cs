namespace Tadawi.Models;

public class Content
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Article, Video, Document, etc.
    public string Status { get; set; } = "draft"; // draft, published, archived
    public string? ThumbnailUrl { get; set; }
    public string? FileUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
