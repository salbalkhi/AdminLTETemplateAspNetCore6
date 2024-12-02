namespace Tadawi.Models.DTOs;

public class AttachmentResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public UserPreviewResponse Uploader { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
    public int? ContentId { get; set; }
}
