using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    //[ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardBiz _dashboardBiz;

        public DashboardController(IDashboardBiz dashboardBiz)
        {
            _dashboardBiz = dashboardBiz;
        }

        [HttpGet("metrics")]
        [Authorize]
        public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
        {
            var appIdClaim = User.FindFirst("AppId")?.Value;
            if (string.IsNullOrEmpty(appIdClaim) || !Guid.TryParse(appIdClaim, out var appId))
            {
                return Unauthorized("Missing or invalid AppId claim.");
            }

            var metrics = await _dashboardBiz.GetMetricsAsync(appId, cancellationToken);
            return Ok(metrics);
        }

    }
}
