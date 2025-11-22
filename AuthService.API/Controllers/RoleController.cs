using AuthService.API.Extensions;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Configs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    //[ApiExplorerSettings(GroupName = "roles")]
    [ApiController]
    [Route("api/roles")]
    [Authorize(Policy = "CanManageRoles")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleBiz _roleBiz;

        public RoleController(IRoleBiz roleBiz)
        {
            _roleBiz = roleBiz;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var appId = this.GetAppIdFromToken();
            var isGlobalAdmin = this.IsGlobalAdminApp();

            var roles = await _roleBiz.GetAllAsync(appId, isGlobalAdmin, cancellationToken);

            var dtos = roles.Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                AppId = r.AppId,
                AppName = r.App.AppName,
                IsSystemDefined = r.IsSystemDefined,
                Permissions = r.Permissions.Select(p => p.PermissionName).ToList()
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("app/{appId:guid}")]
        public async Task<IActionResult> GetRolesByApp(Guid appId, CancellationToken cancellationToken)
        {
            var callerAppId = this.GetAppIdFromToken();
            if (!this.IsGlobalAdminApp() && callerAppId != appId)
                return Unauthorized(new { message = "You are not authorized to view roles for this app." });

            var roles = await _roleBiz.GetByAppIdAsync(appId, cancellationToken);

            var dtos = roles.Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                AppId = r.AppId,
                AppName = r.App?.AppName ?? string.Empty,
                IsSystemDefined = r.IsSystemDefined,
                Permissions = r.Permissions.Select(p => p.PermissionName).ToList()
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{roleId:guid}")]
        public async Task<IActionResult> GetById(Guid roleId, CancellationToken cancellationToken)
        {
            var appId = this.GetAppIdFromToken();
            var isGlobalAdmin = this.IsGlobalAdminApp();

            var role = await _roleBiz.GetByIdAsync(roleId, cancellationToken);
            if (role == null)
                return NotFound(new { code = "RoleNotFound", message = "Role not found." });

            if (!isGlobalAdmin && role.AppId != appId)
                return Forbid("You are not authorized to access this role.");

            return Ok(new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                AppId = role.AppId,
                AppName = role.App.AppName,
                IsSystemDefined = role.IsSystemDefined,
                Permissions = role.Permissions
                    .OrderBy(p => p.IsSystemDefined)
                    .ThenBy(p => p.PermissionName)
                    .Select(p => p.PermissionName)
                    .ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto, CancellationToken cancellationToken)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.RoleName))
                return BadRequest(new { code = "InvalidPayload", message = "Role name required." });

            var callerAppId = this.GetAppIdFromToken();
            if (!this.IsGlobalAdminApp() && callerAppId != dto.AppId)
                return Unauthorized(new
                {
                    code = "UnauthorizedAccess",
                    message = "You are not authorized to create roles for another app."
                });

            try
            {
                var role = await _roleBiz.CreateRoleAsync(dto.AppId, dto.RoleName.Trim(), cancellationToken);
                return CreatedAtAction(nameof(GetById), new { roleId = role.RoleId }, new RoleDto
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    AppId = role.AppId,
                    AppName = string.Empty,
                    IsSystemDefined = role.IsSystemDefined,
                    Permissions = new List<string>()
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { code = "RoleConflict", message = ex.Message });
            }
        }

        [HttpPut("{roleId:guid}")]
        public async Task<IActionResult> Update(Guid roleId, [FromBody] UpdateRoleDto dto, CancellationToken cancellationToken)
        {
            if (dto is null)
                return BadRequest();

            var appId = this.GetAppIdFromToken();
            var role = await _roleBiz.GetByIdAsync(roleId, cancellationToken);
            if (role == null)
                return NotFound(new { message = "Role not found." });

            if (role.IsSystemDefined)
                return BadRequest(new { message = "Cannot modify system-defined role." });

            if (!this.IsGlobalAdminApp() && role.AppId != appId)
                return Forbid("You are not authorized to update this role.");

            var updated = await _roleBiz.UpdateRoleNameAsync(roleId, dto.RoleName?.Trim() ?? role.RoleName, cancellationToken);

            return Ok(new RoleDto
            {
                RoleId = updated.RoleId,
                RoleName = updated.RoleName,
                AppId = updated.AppId,
                AppName = role.App?.AppName ?? string.Empty,
                IsSystemDefined = updated.IsSystemDefined,
                Permissions = updated.Permissions.Select(p => p.PermissionName).ToList()
            });
        }

        [HttpDelete("{roleId:guid}")]
        public async Task<IActionResult> Delete(Guid roleId, CancellationToken cancellationToken)
        {
            var appId = this.GetAppIdFromToken();
            var role = await _roleBiz.GetByIdAsync(roleId, cancellationToken);

            if (role == null)
                return NotFound(new { message = "Role not found." });

            if (role.IsSystemDefined)
                return BadRequest(new { message = "Cannot delete system-defined role." });

            if (!this.IsGlobalAdminApp() && role.AppId != appId)
                return Forbid("You are not authorized to delete this role.");

            await _roleBiz.DeleteRoleAsync(roleId, cancellationToken);
            return NoContent();
        }

        [HttpGet("{roleId:guid}/permissions")]
        public async Task<IActionResult> GetRolePermissions(Guid roleId, CancellationToken cancellationToken)
        {
            var appId = this.GetAppIdFromToken();
            var role = await _roleBiz.GetWithPermissionsAsync(roleId, cancellationToken);

            if (role == null)
                return NotFound(new { message = "Role not found." });

            if (!this.IsGlobalAdminApp() && role.AppId != appId)
                return Forbid("You are not authorized to view this role’s permissions.");

            var permissions = role.Permissions
                .Select(p => new PermissionDto
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    IsSystemDefined = p.IsSystemDefined
                })
                .ToList();

            return Ok(permissions);
        }

        [HttpGet("{roleId:guid}/users")]
        public async Task<IActionResult> GetRoleUsers(Guid roleId, CancellationToken cancellationToken)
        {
            var appId = this.GetAppIdFromToken();
            var role = await _roleBiz.GetWithUsersAsync(roleId, cancellationToken);

            if (role == null)
                return NotFound(new { message = "Role not found." });

            if (!this.IsGlobalAdminApp() && role.AppId != appId)
                return Forbid("You are not authorized to view this role’s users.");

            var users = role.Users
                .Where(u => !u.IsDeleted)
                .Select(u =>
                {
                    var status = u.AppStatuses
                        .FirstOrDefault(s => s.AppId == appId)?.Status ?? AppAccountStatus.Pending;

                    return new UserListDto
                    {
                        UserId = u.UserId,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        AccountStatus = status.ToString(),
                        IsDeleted = u.IsDeleted,
                        CreatedAt = u.CreatedAt
                    };
                })
                .ToList();

            return Ok(users);
        }

    }
}
