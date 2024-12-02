using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Tadawi.Data;

namespace Tadawi.Middleware;

public class UserActivityMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly TimeSpan ActivityThreshold = TimeSpan.FromMinutes(15);

    public UserActivityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var user = await dbContext.Users.FindAsync(userId);
                if (user != null)
                {
                    var now = DateTime.UtcNow;
                    var shouldUpdate = !user.LastActiveAt.HasValue || 
                                     (now - user.LastActiveAt.Value) > ActivityThreshold;

                    if (shouldUpdate)
                    {
                        user.LastActiveAt = now;
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }

        await _next(context);
    }
}
