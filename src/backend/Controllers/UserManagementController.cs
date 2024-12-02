using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Tadawi.Data;
using Tadawi.Models;
using Tadawi.Models.DTOs;
using Tadawi.Services;

namespace Tadawi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserManagementController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;

    public UserManagementController(ApplicationDbContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    [HttpGet("preview")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<UserPreviewResponse>>> GetUserPreviews()
    {
        var users = await _context.Users
            .Select(u => new UserPreviewResponse
            {
                Id = u.Id,
                Username = u.Username,
                IsActive = u.IsActive,
                LastActiveAt = u.LastActiveAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("active-users")]
    public async Task<ActionResult<IEnumerable<UserPreviewResponse>>> GetActiveUsers()
    {
        var fifteenMinutesAgo = DateTime.UtcNow.AddMinutes(-15);
        
        var activeUsers = await _context.Users
            .Where(u => u.IsActive && u.LastActiveAt.HasValue && u.LastActiveAt > fifteenMinutesAgo)
            .Select(u => new UserPreviewResponse
            {
                Id = u.Id,
                Username = u.Username,
                IsActive = u.IsActive,
                LastActiveAt = u.LastActiveAt
            })
            .ToListAsync();

        return Ok(activeUsers);
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Verify current password
        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect" });

        // Check username uniqueness if it's being updated
        if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { message = "Username is already taken" });
            user.Username = request.Username;
        }

        // Check email uniqueness if it's being updated
        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "Email is already registered" });
            user.Email = request.Email;
        }

        // Update password if provided
        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        }

        await _context.SaveChangesAsync();

        return Ok(new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }

    [HttpDelete("profile")]
    public async Task<IActionResult> DeleteProfile([FromBody] DeleteUserRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Verify password before deletion
        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            return BadRequest(new { message = "Password is incorrect" });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User account deleted successfully" });
    }
}
