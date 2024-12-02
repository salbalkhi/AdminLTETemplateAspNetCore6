using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tadawi.Data;
using Tadawi.Models;
using Tadawi.Services;

namespace Tadawi.Controllers;

/// <summary>
/// Controller for handling authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;

    public AuthController(
        IOptions<JwtSettings> jwtSettings,
        ApplicationDbContext context,
        IPasswordService passwordService)
    {
        _jwtSettings = jwtSettings.Value;
        _context = context;
        _passwordService = passwordService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="request">The login credentials</param>
    /// <returns>JWT token and user information if authentication is successful</returns>
    /// <response code="200">Returns the JWT token</response>
    /// <response code="401">If the credentials are invalid</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            // For demo purposes, create a new user if it's the demo account
            if (request.Username == "demo" && request.Password == "demo123")
            {
                user = new User
                {
                    Username = request.Username,
                    PasswordHash = _passwordService.HashPassword(request.Password)
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                return Unauthorized("Invalid username or password");
            }
        }
        else if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return BadRequest(new { message = "Invalid username or password" });
        }

        var now = DateTime.UtcNow;
        user.LastLoginAt = now;
        user.LastActiveAt = now;
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user.Username);
        var refreshToken = await GenerateRefreshTokenAsync(user);
        var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

        return Ok(new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            Username = user.Username,
            Expiration = expiration
        });
    }

    /// <summary>
    /// Refreshes an expired JWT token using a valid refresh token
    /// </summary>
    /// <param name="request">The refresh token request</param>
    /// <returns>A new JWT token and refresh token</returns>
    /// <response code="200">Returns the new JWT token</response>
    /// <response code="401">If the refresh token is invalid or expired</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && !r.IsRevoked);

        if (refreshToken == null)
        {
            return Unauthorized("Invalid refresh token");
        }

        if (refreshToken.ExpiryDate < DateTime.UtcNow)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync();
            return Unauthorized("Refresh token expired");
        }

        var token = GenerateJwtToken(refreshToken.User.Username);
        var newRefreshToken = await GenerateRefreshTokenAsync(refreshToken.User);
        var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

        // Revoke the old refresh token
        refreshToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            Token = token,
            RefreshToken = newRefreshToken.Token,
            Username = refreshToken.User.Username,
            Expiration = expiration
        });
    }

    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="request">The registration request</param>
    /// <returns>Access token, refresh token, and user information</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest(new { message = "Username is already taken" });

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "Email is already registered" });

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordService.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate tokens
        var accessToken = GenerateJwtToken(user.Username);
        var refreshToken = await GenerateRefreshTokenAsync(user);

        return Ok(new
        {
            accessToken,
            refreshToken = refreshToken.Token,
            user = new { user.Id, user.Username, user.Email }
        });
    }

    private string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(User user)
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);

        var token = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays)
        };

        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();

        return token;
    }
}
