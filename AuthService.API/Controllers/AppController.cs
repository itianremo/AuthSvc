using AuthService.API.Extensions;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Configs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [ApiExplorerSettings(GroupName = "apps")]
    [ApiController]
    [Route("api/apps")]
    [Authorize]
    public class AppController : ControllerBase
    {
        private readonly IAppBiz _appBiz;
        private readonly IUserAppStatusBiz _userAppStatusBiz;

        public AppController(IAppBiz appBiz, IUserAppStatusBiz userAppStatusBiz)
        {
            _appBiz = appBiz;
            _userAppStatusBiz = userAppStatusBiz;
        }

        [HttpGet]
        [Authorize(Policy = "CanManageApps")]
        public async Task<IActionResult> GetAllApps(CancellationToken cancellationToken)
        {
            if (!this.IsGlobalAdminApp())
            {
                var callerAppId = this.GetAppIdFromToken();
                if (callerAppId == null)
                    return Forbid("Missing AppId claim.");

                var app = await _appBiz.GetWithStatusesAsync(callerAppId.Value, cancellationToken);
                return Ok(new[] { app });
            }

            var apps = await _appBiz.GetAllAppsAsync(cancellationToken);
            return Ok(apps);
        }

        [HttpGet("{appId:guid}")]
        [Authorize(Policy = "CanManageApps")]
        public async Task<IActionResult> GetAppById(Guid appId, CancellationToken cancellationToken)
        {
            if (!this.IsGlobalAdminApp() && this.GetAppIdFromToken() != appId)
                return Forbid("You are not authorized to access this app.");

            var app = await _appBiz.GetWithStatusesAsync(appId, cancellationToken);
            if (app == null)
                return NotFound(new { code = "AppNotFound", message = "App not found." });

            return Ok(app);
        }

        [HttpPost]
        [Authorize(Policy = "CanManageApps")]
        public async Task<IActionResult> CreateApp([FromBody] CreateAppDto dto, CancellationToken cancellationToken)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.AppName))
                return BadRequest(new { code = "InvalidPayload", message = "App name required." });

            if (!this.IsGlobalAdminApp())
                return Forbid("Only global admin can create apps.");

            try
            {
                var app = await _appBiz.CreateAppAsync(dto.AppName, dto.RedirectUrls ?? string.Empty, dto.Scopes ?? string.Empty, dto.AutoApproveUsers, cancellationToken);
                return CreatedAtAction(nameof(GetAppById), new { appId = app.AppId }, app);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { code = "DuplicateName", message = ex.Message });
            }
        }

        [HttpPut("{appId:guid}")]
        [Authorize(Policy = "CanManageApps")]
        public async Task<IActionResult> UpdateApp(Guid appId, [FromBody] UpdateAppDto dto, CancellationToken cancellationToken)
        {
            if (dto is null)
                return BadRequest();

            if (!this.IsGlobalAdminApp() && this.GetAppIdFromToken() != appId)
                return Forbid();

            var app = await _appBiz.UpdateAppAsync(appId, dto.AppName, dto.RedirectUrls, dto.Scopes, dto.AutoApproveUsers, cancellationToken);
            return Ok(app);
        }

        [HttpDelete("{appId:guid}")]
        [Authorize(Policy = "CanManageApps")]
        public async Task<IActionResult> DeleteApp(Guid appId, CancellationToken cancellationToken)
        {
            if (!this.IsGlobalAdminApp() && this.GetAppIdFromToken() != appId)
                return Forbid();

            await _appBiz.DeleteAppAsync(appId, cancellationToken);
            return NoContent();
        }

        [HttpPost("{appId:guid}/undelete")]
        [Authorize(Policy = "CanManageApps")]
        public async Task<IActionResult> UndeleteApp(Guid appId, CancellationToken cancellationToken)
        {
            if (!this.IsGlobalAdminApp() && this.GetAppIdFromToken() != appId)
                return Forbid();

            var app = await _appBiz.UndeleteAppAsync(appId, cancellationToken);
            return Ok(app);
        }

        [HttpGet("{appId:guid}/pending-users")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> GetPendingUsers(Guid appId, CancellationToken cancellationToken)
        {
            if (!this.IsGlobalAdminApp() && this.GetAppIdFromToken() != appId)
                return Forbid();

            var users = await _userAppStatusBiz.GetPendingUsersAsync(appId, cancellationToken);
            return Ok(users);
        }

        [HttpPost("approve-user")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> ApproveUser([FromBody] ApproveUserDto dto, CancellationToken cancellationToken)
        {
            if (dto is null)
                return BadRequest();

            if (!this.IsGlobalAdminApp() && this.GetAppIdFromToken() != dto.AppId)
                return Forbid();

            await _userAppStatusBiz.UpdateStatusAsync(dto.UserId, dto.AppId, AppAccountStatus.Approved, cancellationToken);
            return Ok(new { message = "User approved successfully." });
        }

        [HttpPost("update-user-status")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateUserAppStatusDto dto, CancellationToken cancellationToken)
        {
            if (dto is null)
                return BadRequest(new { code = "InvalidPayload", message = "Request body is required." });

            if (!this.IsGlobalAdminApp() && this.GetAppIdFromToken() != dto.AppId)
                return Forbid();

            // Convert string to enum safely
            if (!Enum.TryParse<AppAccountStatus>(dto.NewStatus, true, out var statusEnum))
                return BadRequest(new { code = "InvalidStatus", message = $"Invalid status value '{dto.NewStatus}'." });

            await _userAppStatusBiz.UpdateStatusAsync(dto.UserId, dto.AppId, statusEnum, cancellationToken);

            return Ok(new { message = $"User status updated to {statusEnum}." });
        }

        [HttpGet("{appId:guid}/users")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> GetUsersForApp(Guid appId, CancellationToken cancellationToken)
        {
            if (!this.IsGlobalAdminApp() && this.GetAppIdFromToken() != appId)
                return Forbid();

            var users = await _userAppStatusBiz.GetByAppIdAsync(appId, cancellationToken);
            return Ok(users);
        }
    }
}
