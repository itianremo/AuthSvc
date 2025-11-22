using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.API.Middlewares
{
    public class AccountStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public AccountStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AuthDbContext db)
        {
            var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                              ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var appIdClaim = context.User.FindFirst("AppId")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId) && Guid.TryParse(appIdClaim, out var appId))
            {
                var user = await db.Users
                    .Include(u => u.AppStatuses)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user != null)
                {
                    var status = user.AppStatuses.FirstOrDefault(s => s.AppId == appId)?.Status;
                    if (!string.IsNullOrEmpty(status.ToString()))
                    {
                        context.Items["AccountStatus"] = status;
                    }
                }
            }

            await _next(context);
        }
    }
}
