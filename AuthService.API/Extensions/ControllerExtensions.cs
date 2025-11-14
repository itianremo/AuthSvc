using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.API.Extensions
{
    public static class ControllerExtensions
    {
        public static Guid? GetAppIdFromToken(this ControllerBase controller)
        {
            var appIdClaim = controller.User.FindFirst("AppId")?.Value;
            return Guid.TryParse(appIdClaim, out var appId) ? appId : null;
        }

        public static Guid GetSeededAppId(this ControllerBase controller)
        {
            var appIdStr = controller.HttpContext.RequestServices
                .GetService<IConfiguration>()?["Init:Seeds:appId"];
            return Guid.TryParse(appIdStr, out var seededAppId) ? seededAppId : Guid.Empty;
        }

        public static bool IsGlobalAdminApp(this ControllerBase controller)
        {
            var appId = controller.GetAppIdFromToken();
            return appId.HasValue && appId.Value == controller.GetSeededAppId();
        }
    }
}