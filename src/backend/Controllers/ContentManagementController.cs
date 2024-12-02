using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tadawi.Data;
using Tadawi.Models;
using Tadawi.Models.DTOs;

namespace Tadawi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentManagementController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ContentManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContentResponse>>> GetContents(
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Contents
            .Include(c => c.Author)
            .AsQueryable();

        if (!string.IsNullOrEmpty(type))
            query = query.Where(c => c.Type == type);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(c => c.Status == status);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var contents = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ContentResponse
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Body = c.Body,
                Type = c.Type,
                Status = c.Status,
                ThumbnailUrl = c.ThumbnailUrl,
                FileUrl = c.FileUrl,
                Tags = c.Tags,
                Author = new UserPreviewResponse
                {
                    Id = c.Author.Id,
                    Username = c.Author.Username,
                    IsActive = c.Author.IsActive,
                    LastActiveAt = c.Author.LastActiveAt
                },
                CreatedAt = c.CreatedAt,
                PublishedAt = c.PublishedAt,
                LastModifiedAt = c.LastModifiedAt
            })
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalItems.ToString());
        Response.Headers.Add("X-Total-Pages", totalPages.ToString());

        return Ok(contents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContentResponse>> GetContent(int id)
    {
        var content = await _context.Contents
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (content == null)
            return NotFound(new { message = "Content not found" });

        return Ok(new ContentResponse
        {
            Id = content.Id,
            Title = content.Title,
            Description = content.Description,
            Body = content.Body,
            Type = content.Type,
            Status = content.Status,
            ThumbnailUrl = content.ThumbnailUrl,
            FileUrl = content.FileUrl,
            Tags = content.Tags,
            Author = new UserPreviewResponse
            {
                Id = content.Author.Id,
                Username = content.Author.Username,
                IsActive = content.Author.IsActive,
                LastActiveAt = content.Author.LastActiveAt
            },
            CreatedAt = content.CreatedAt,
            PublishedAt = content.PublishedAt,
            LastModifiedAt = content.LastModifiedAt
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ContentResponse>> CreateContent([FromBody] ContentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized(new { message = "User not found" });

        var content = new Content
        {
            Title = request.Title,
            Description = request.Description,
            Body = request.Body,
            Type = request.Type,
            ThumbnailUrl = request.ThumbnailUrl,
            FileUrl = request.FileUrl,
            Tags = request.Tags ?? new List<string>(),
            AuthorId = userId,
            Status = "draft"
        };

        _context.Contents.Add(content);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContent), new { id = content.Id }, new ContentResponse
        {
            Id = content.Id,
            Title = content.Title,
            Description = content.Description,
            Body = content.Body,
            Type = content.Type,
            Status = content.Status,
            ThumbnailUrl = content.ThumbnailUrl,
            FileUrl = content.FileUrl,
            Tags = content.Tags,
            Author = new UserPreviewResponse
            {
                Id = user.Id,
                Username = user.Username,
                IsActive = user.IsActive,
                LastActiveAt = user.LastActiveAt
            },
            CreatedAt = content.CreatedAt,
            PublishedAt = content.PublishedAt,
            LastModifiedAt = content.LastModifiedAt
        });
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContent(int id, [FromBody] ContentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var content = await _context.Contents.FindAsync(id);

        if (content == null)
            return NotFound(new { message = "Content not found" });

        if (content.AuthorId != userId)
            return Forbid();

        content.Title = request.Title;
        content.Description = request.Description;
        content.Body = request.Body;
        content.Type = request.Type;
        content.ThumbnailUrl = request.ThumbnailUrl;
        content.FileUrl = request.FileUrl;
        content.Tags = request.Tags ?? new List<string>();
        content.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateContentStatus(int id, [FromBody] string status)
    {
        if (!new[] { "draft", "published", "archived" }.Contains(status))
            return BadRequest(new { message = "Invalid status" });

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var content = await _context.Contents.FindAsync(id);

        if (content == null)
            return NotFound(new { message = "Content not found" });

        if (content.AuthorId != userId)
            return Forbid();

        content.Status = status;
        content.LastModifiedAt = DateTime.UtcNow;

        if (status == "published" && !content.PublishedAt.HasValue)
            content.PublishedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContent(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var content = await _context.Contents.FindAsync(id);

        if (content == null)
            return NotFound(new { message = "Content not found" });

        if (content.AuthorId != userId)
            return Forbid();

        _context.Contents.Remove(content);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
