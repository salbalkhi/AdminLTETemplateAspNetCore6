namespace Tadawi.Models;

public class Attachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public int UploaderId { get; set; }
    public User Uploader { get; set; } = null!;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int? ContentId { get; set; }
    public Content? Content { get; set; }
}
