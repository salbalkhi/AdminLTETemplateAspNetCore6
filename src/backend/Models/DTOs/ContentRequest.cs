using System.ComponentModel.DataAnnotations;

namespace Tadawi.Models.DTOs;

public class ContentRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Article|Video|Document)$", ErrorMessage = "Type must be either 'Article', 'Video', or 'Document'")]
    public string Type { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }
    
    public string? FileUrl { get; set; }

    public List<string>? Tags { get; set; }
}
