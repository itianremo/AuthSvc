using AuthService.Application.Helpers;
using AuthService.Infrastructure.Data;
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
            //var userIdClaim = context.User.FindFirst("sub")?.Value;
            var userIdClaim = context.User.FindFirst("user_id")?.Value
                   ?? context.User.FindFirst("sub")?.Value
                   ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var user = await db.Users.FindAsync(userId);
                if (user != null)
                {
                    user.GlobalAccountStatus = AccountStatusHelper.GetGlobalStatus(user);
                    await db.SaveChangesAsync();
                }
            }

            await _next(context);
        }
    }
}
