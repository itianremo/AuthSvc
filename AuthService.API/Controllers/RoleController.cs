using AuthService.API.Extensions;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize(Policy = "CanManageRoles")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleBiz _roleBiz;
        private readonly IUserRoleBiz _userRoleBiz;

        public RoleController(IRoleBiz roleBiz, IUserRoleBiz userRoleBiz)
        {
            _roleBiz = roleBiz;
            _userRoleBiz = userRoleBiz;
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var appId = this.GetAppIdFromToken();

            IQueryable<Role> query = _roleBiz.RoleRepository.Query().AsNoTracking();

            if (!this.IsGlobalAdminApp())
            {
                query = query.Where(r => r.AppId == appId);
            }

            var roles = await query
                .Select(r => new RoleDto
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    AppId = r.AppId,
                    IsSystemDefined = r.IsSystemDefined,
                    Permissions = new List<string>()
                })
                .ToListAsync();

            return Ok(roles);
        }

        /// <summary>
        /// Get roles by app
        /// </summary>
        [HttpGet("app/{appId:guid}")]
        public async Task<IActionResult> GetRolesByApp(Guid appId)
        {
            var callerAppId = this.GetAppIdFromToken();

            // If caller is NOT global admin, enforce that they can only query their own app
            if (!this.IsGlobalAdminApp() && callerAppId != appId)
            {
                return Unauthorized(new { message = "You are not authorized to view roles for this app." });
            }

            var roles = await _roleBiz.RoleRepository.Query()
                .Where(r => r.AppId == appId)
                .Include(rp => rp.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .AsNoTracking()
                .Select(r => new RoleDto
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    AppId = r.AppId,
                    IsSystemDefined = r.IsSystemDefined,
                    Permissions = r.RolePermissions.Select(rp => rp.Permission.PermissionName).ToList()
                })
                .ToListAsync();

            return Ok(roles);
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        [HttpGet("{roleId:guid}")]
        public async Task<IActionResult> GetById(Guid roleId)
        {
            var appId = this.GetAppIdFromToken();

            var role = await _roleBiz.RoleRepository.Query()
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role == null)
                return NotFound(new { message = "Role not found." });

            if (!this.IsGlobalAdminApp() && role.AppId != appId)
                return Forbid("You are not authorized to access this role.");

            return Ok(new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                AppId = role.AppId,
                IsSystemDefined = role.IsSystemDefined,
                Permissions = role.RolePermissions.Select(rp => rp.Permission.PermissionName).ToList()
            });
        }

        /// <summary>
        /// Create new role
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.RoleName))
                return BadRequest(new { message = "Role name required." });

            var callerAppId = this.GetAppIdFromToken();

            // If caller is not global admin, enforce that they can only create roles in their own app
            if (!this.IsGlobalAdminApp() && callerAppId != dto.AppId)
            {
                return Unauthorized(new { message = "You are not authorized to create roles for another app." });
            }

            var exists = await _roleBiz.RoleRepository.Query()
                .AnyAsync(r => r.RoleName == dto.RoleName && r.AppId == dto.AppId);
            if (exists)
                return Conflict(new { message = "Role already exists for this app." });

            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = dto.RoleName,
                AppId = dto.AppId,
                IsSystemDefined = false
            };

            await _roleBiz.RoleRepository.AddAsync(role);
            await _roleBiz.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { roleId = role.RoleId }, new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                AppId = role.AppId,
                IsSystemDefined = role.IsSystemDefined,
                Permissions = new List<string>()
            });
        }

        /// <summary>
        /// Update role
        /// </summary>
        [HttpPut("{roleId:guid}")]
        public async Task<IActionResult> Update(Guid roleId, [FromBody] UpdateRoleDto dto)
        {
            if (dto is null)
                return BadRequest();

            var appId = this.GetAppIdFromToken();

            var role = await _roleBiz.RoleRepository.GetByIdAsync(roleId);
            if (role == null)
                return NotFound(new { message = "Role not found." });

            if (role.IsSystemDefined)
                return BadRequest(new { message = "Cannot modify system-defined role." });

            if (!this.IsGlobalAdminApp() && role.AppId != appId)
                return Forbid("You are not authorized to update this role.");

            if (!string.IsNullOrWhiteSpace(dto.RoleName))
                role.RoleName = dto.RoleName;

            _roleBiz.RoleRepository.Update(role);
            await _roleBiz.SaveChangesAsync();

            return Ok(new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                AppId = role.AppId,
                IsSystemDefined = role.IsSystemDefined,
                Permissions = new List<string>()
            });
        }

        /// <summary>
        /// Delete role
        /// </summary>
        [HttpDelete("{roleId:guid}")]
        public async Task<IActionResult> Delete(Guid roleId)
        {
            var appId = this.GetAppIdFromToken();

            var role = await _roleBiz.RoleRepository.Query()
                .Include(r => r.RolePermissions)
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role == null)
                return NotFound(new { message = "Role not found." });

            if (role.IsSystemDefined)
                return BadRequest(new { message = "Cannot delete system-defined role." });

            if (!this.IsGlobalAdminApp() && role.AppId != appId)
                return Forbid("You are not authorized to delete this role.");

            // Step 1: Remove UserRole assignments
            if (role.UserRoles.Any())
            {
                foreach (var ur in role.UserRoles.ToList())
                {
                    _userRoleBiz.UserRoleRepository.Remove(ur);
                }
                await _userRoleBiz.SaveChangesAsync();
            }

            // Step 2: Remove RolePermission assignments
            if (role.RolePermissions.Any())
            {
                foreach (var rp in role.RolePermissions.ToList())
                {
                    _roleBiz.RolePermissionRepository.Remove(rp);
                }
                await _roleBiz.SaveChangesAsync();
            }

            // Step 3: Remove the role itself
            _roleBiz.RoleRepository.Remove(role);
            await _roleBiz.SaveChangesAsync();

            return NoContent();
        }
    }
}