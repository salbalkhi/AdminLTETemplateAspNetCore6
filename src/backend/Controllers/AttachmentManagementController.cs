using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tadawi.Data;
using Tadawi.Models;
using Tadawi.Models.DTOs;
using Tadawi.Services;

namespace Tadawi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttachmentManagementController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;
    private readonly long _maxFileSize = 100 * 1024 * 1024; // 100MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

    public AttachmentManagementController(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttachmentResponse>>> GetAttachments(
        [FromQuery] string? contentType,
        [FromQuery] int? contentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Attachments
            .Include(a => a.Uploader)
            .AsQueryable();

        if (!string.IsNullOrEmpty(contentType))
            query = query.Where(a => a.ContentType.StartsWith(contentType));

        if (contentId.HasValue)
            query = query.Where(a => a.ContentId == contentId);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var attachments = await query
            .OrderByDescending(a => a.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AttachmentResponse
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                PublicUrl = a.PublicUrl,
                Uploader = new UserPreviewResponse
                {
                    Id = a.Uploader.Id,
                    Username = a.Uploader.Username,
                    IsActive = a.Uploader.IsActive,
                    LastActiveAt = a.Uploader.LastActiveAt
                },
                UploadedAt = a.UploadedAt,
                ContentId = a.ContentId
            })
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalItems.ToString());
        Response.Headers.Add("X-Total-Pages", totalPages.ToString());

        return Ok(attachments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AttachmentResponse>> GetAttachment(int id)
    {
        var attachment = await _context.Attachments
            .Include(a => a.Uploader)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attachment == null)
            return NotFound(new { message = "Attachment not found" });

        return Ok(new AttachmentResponse
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            PublicUrl = attachment.PublicUrl,
            Uploader = new UserPreviewResponse
            {
                Id = attachment.Uploader.Id,
                Username = attachment.Uploader.Username,
                IsActive = attachment.Uploader.IsActive,
                LastActiveAt = attachment.Uploader.LastActiveAt
            },
            UploadedAt = attachment.UploadedAt,
            ContentId = attachment.ContentId
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AttachmentResponse>> UploadAttachment(
        [FromForm] IFormFile file,
        [FromForm] int? contentId = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file was uploaded" });

        if (file.Length > _maxFileSize)
            return BadRequest(new { message = "File size exceeds the limit of 100MB" });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return BadRequest(new { message = "File type is not allowed" });

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized(new { message = "User not found" });

        // If contentId is provided, verify it exists
        if (contentId.HasValue)
        {
            var content = await _context.Contents.FindAsync(contentId.Value);
            if (content == null)
                return BadRequest(new { message = "Content not found" });
        }

        try
        {
            var (storagePath, publicUrl) = await _storageService.SaveFileAsync(file, "attachments");

            var attachment = new Attachment
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                StoragePath = storagePath,
                PublicUrl = publicUrl,
                UploaderId = userId,
                ContentId = contentId
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAttachment), new { id = attachment.Id }, new AttachmentResponse
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                PublicUrl = attachment.PublicUrl,
                Uploader = new UserPreviewResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    IsActive = user.IsActive,
                    LastActiveAt = user.LastActiveAt
                },
                UploadedAt = attachment.UploadedAt,
                ContentId = attachment.ContentId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error uploading file", error = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var attachment = await _context.Attachments.FindAsync(id);

        if (attachment == null)
            return NotFound(new { message = "Attachment not found" });

        if (attachment.UploaderId != userId)
            return Forbid();

        try
        {
            await _storageService.DeleteFileAsync(attachment.StoragePath);
            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting file", error = ex.Message });
        }
    }
}
