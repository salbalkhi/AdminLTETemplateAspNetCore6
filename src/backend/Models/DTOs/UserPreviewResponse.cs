namespace Tadawi.Models.DTOs;

public class UserPreviewResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastActiveAt { get; set; }
}
