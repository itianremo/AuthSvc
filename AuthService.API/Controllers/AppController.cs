using AuthService.Application.DTOs;
using AuthService.Application.DTOs.AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/apps")]
    [Authorize]
    public class AppController : ControllerBase
    {
        private readonly IAppBiz _appBiz;
        private readonly IUserAppBiz _userAppBiz;

        public AppController(IAppBiz appBiz, IUserAppBiz userAppBiz)
        {
            _appBiz = appBiz;
            _userAppBiz = userAppBiz;
        }

        /// <summary>
        /// Get all apps
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "CanManageApps")]
        public async Task<IActionResult> GetAllApps()
        {
            var apps = await _appBiz.AppRepository.Query()
                .AsNoTracking()
                .ToListAsync();

            return Ok(apps);
        }

        /// <summary>
        /// Get app by ID
        /// </summary>
        [HttpGet("{appId:guid}")]
        [Authorize(Policy = "CanManageApps")]
        public async Task<IActionResult> GetAppById(Guid appId)
        {
            var app = await _appBiz.AppRepository.GetByIdAsync(appId);

            if (app == null)
                return NotFound(new { message = "App not found." });

            return Ok(app);
        }

        /// <summary>
        /// Create new app
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "CanManageApps")]
        public async Task<IActionResult> CreateApp([FromBody] CreateAppDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.AppName))
                return BadRequest(new { message = "App name required." });

            var app = new App
            {
                AppId = Guid.NewGuid(),
                AppName = dto.AppName,
                RedirectUrls = dto.RedirectUrls ?? string.Empty,
                Scopes = dto.Scopes ?? string.Empty,
                AutoApproveUsers = dto.AutoApproveUsers,
                IsCoreApp = false
            };

            await _appBiz.AppRepository.AddAsync(app);
            await _appBiz.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAppById), new { appId = app.AppId }, app);
        }

        /// <summary>
        /// Get pending users for app
        /// </summary>
        [HttpGet("{appId:guid}/pending-users")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> GetPendingUsers(Guid appId)
        {
            var app = await _appBiz.AppRepository.GetByIdAsync(appId);
            if (app == null)
                return NotFound(new { message = "App not found." });

            var users = await _userAppBiz.UserAppRepository.Query()
                .Where(ua => ua.AppId == appId && ua.AccountStatus == AccountStatus.Pending)
                .Include(ua => ua.User)
                .AsNoTracking()
                .Select(ua => new
                {
                    ua.User.UserId,
                    ua.User.Email,
                    ua.User.PhoneNumber,
                    ua.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Approve user for app
        /// </summary>
        [HttpPost("approve-user")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> ApproveUser([FromBody] ApproveUserDto dto)
        {
            if (dto is null)
                return BadRequest();

            var userApp = await _userAppBiz.UserAppRepository.Query()
                .FirstOrDefaultAsync(ua => ua.UserId == dto.UserId && ua.AppId == dto.AppId);

            if (userApp == null)
                return NotFound(new { message = "User not registered for this app." });

            userApp.AccountStatus = AccountStatus.Approved;
            _userAppBiz.UserAppRepository.Update(userApp);
            await _userAppBiz.SaveChangesAsync();

            return Ok(new { message = "User approved successfully." });
        }

        /// <summary>
        /// Update user app status
        /// </summary>
        [HttpPost("update-user-status")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateUserAppStatusDto dto)
        {
            if (dto is null)
                return BadRequest();

            var userApp = await _userAppBiz.UserAppRepository.Query()
                .FirstOrDefaultAsync(ua => ua.UserId == dto.UserId && ua.AppId == dto.AppId);

            if (userApp == null)
                return NotFound(new { message = "User not registered for this app." });

            userApp.AccountStatus = dto.NewStatus;
            _userAppBiz.UserAppRepository.Update(userApp);
            await _userAppBiz.SaveChangesAsync();

            return Ok(new { message = $"User status updated to {dto.NewStatus}." });
        }
    }
}