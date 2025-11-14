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
    [Route("api/permissions")]
    [Authorize(Policy = "CanManagePermissions")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionBiz _permissionBiz;
        private readonly IRolePermissionBiz _rolePermissionBiz;
        private readonly IConfiguration _config;

        public PermissionController(IPermissionBiz permissionBiz,
            IRolePermissionBiz rolePermissionBiz,
            IConfiguration config)
        {
            _permissionBiz = permissionBiz;
            _rolePermissionBiz = rolePermissionBiz;
            _config = config;
        }

        /// <summary>
        /// Get all permissions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var appId = this.GetAppIdFromToken();

            if (this.IsGlobalAdminApp())
            {
                // Return all permissions
                var allPermissions = await _permissionBiz.PermissionRepository.Query()
                    .AsNoTracking()
                    .Select(p => new PermissionListItemDto
                    {
                        PermissionId = p.PermissionId,
                        PermissionName = p.PermissionName,
                        IsSystemDefined = p.IsSystemDefined
                    })
                    .ToListAsync();

                return Ok(allPermissions);
            }

            // Filter permissions by roles in this app
            var permissionIds = await _rolePermissionBiz.RolePermissionRepository.Query()
                .Where(rp => rp.Role.AppId == appId)
                .Select(rp => rp.PermissionId)
                .Distinct()
                .ToListAsync();

            var scopedPermissions = await _permissionBiz.PermissionRepository.Query()
                .Where(p => permissionIds.Contains(p.PermissionId))
                .AsNoTracking()
                .Select(p => new PermissionListItemDto
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    IsSystemDefined = p.IsSystemDefined
                })
                .ToListAsync();

            return Ok(scopedPermissions);
        }

        /// <summary>
        /// Get permission by ID
        /// </summary>
        [HttpGet("{permissionId:guid}")]
        public async Task<IActionResult> GetById(Guid permissionId)
        {
            var appId = this.GetAppIdFromToken();

            var permission = await _permissionBiz.PermissionRepository.Query()
                .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.PermissionId == permissionId);

            if (permission == null)
                return NotFound(new { message = "Permission not found." });

            if (!this.IsGlobalAdminApp() && !permission.RolePermissions.Any(rp => rp.Role.AppId == appId))
                return Forbid("You are not authorized to access this permission.");

            return Ok(new PermissionDto
            {
                PermissionId = permission.PermissionId,
                PermissionName = permission.PermissionName,
                IsSystemDefined = permission.IsSystemDefined,
                RoleId = permission.RolePermissions
                            .Where(rp => rp.Role.AppId == appId)
                            .Select(rp => rp.Role.RoleId)
                            .FirstOrDefault(),
                RoleName = permission.RolePermissions
                            .Where(rp => rp.Role.AppId == appId)
                            .Select(rp => rp.Role.RoleName)
                            .FirstOrDefault()
            });
        }

        /// <summary>
        /// Create new permission
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePermissionDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.PermissionName))
                return BadRequest(new { message = "Permission name required." });

            var exists = await _permissionBiz.PermissionRepository.Query()
                .AnyAsync(p => p.PermissionName == dto.PermissionName);
            if (exists)
                return Conflict(new { message = "Permission already exists." });

            var permission = new Permission
            {
                PermissionId = Guid.NewGuid(),
                PermissionName = dto.PermissionName,
                IsSystemDefined = false,
            };

            await _permissionBiz.PermissionRepository.AddAsync(permission);
            await _permissionBiz.SaveChangesAsync();

            await _permissionBiz.RolePermissionRepository.AddAsync(new RolePermission
            {
                RolePermissionId = Guid.NewGuid(),
                RoleId = dto.RoleId,
                PermissionId = permission.PermissionId
            });
            await _permissionBiz.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { permissionId = permission.PermissionId }, new PermissionDto
            {
                PermissionId = permission.PermissionId,
                PermissionName = permission.PermissionName,
                IsSystemDefined = permission.IsSystemDefined,
                RoleId = dto.RoleId,
                RoleName = _rolePermissionBiz.RoleRepository.Query()
                            .Where(r => r.RoleId == dto.RoleId)
                            .Select(r => r.RoleName)
                            .FirstOrDefault()
            });
        }

        /// <summary>
        /// Update permission
        /// </summary>
        [HttpPut("{permissionId:guid}")]
        public async Task<IActionResult> Update(Guid permissionId, [FromBody] CreatePermissionDto dto)
        {
            if (dto is null)
                return BadRequest();

            var appId = this.GetAppIdFromToken();

            var permission = await _permissionBiz.PermissionRepository.Query()
                .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.PermissionId == permissionId);

            if (permission == null)
                return NotFound(new { message = "Permission not found." });

            if (permission.IsSystemDefined)
                return BadRequest(new { message = "Cannot modify system-defined permission." });

            if (!this.IsGlobalAdminApp() && !permission.RolePermissions.Any(rp => rp.Role.AppId == appId))
                return Forbid("You are not authorized to update this permission.");

            if (!string.IsNullOrWhiteSpace(dto.PermissionName))
                permission.PermissionName = dto.PermissionName;

            _permissionBiz.PermissionRepository.Update(permission);
            await _permissionBiz.SaveChangesAsync();

            return Ok(new PermissionListItemDto
            {
                PermissionId = permission.PermissionId,
                PermissionName = permission.PermissionName,
                IsSystemDefined = permission.IsSystemDefined
            });
        }

        /// <summary>
        /// Delete permission
        /// </summary>
        [HttpDelete("{permissionId:guid}")]
        public async Task<IActionResult> Delete(Guid permissionId)
        {
            var appId = this.GetAppIdFromToken();

            var permission = await _permissionBiz.PermissionRepository.Query()
                .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.PermissionId == permissionId);

            if (permission == null)
                return NotFound(new { message = "Permission not found." });

            if (permission.IsSystemDefined)
                return BadRequest(new { message = "Cannot delete system-defined permission." });

            if (!this.IsGlobalAdminApp() && !permission.RolePermissions.Any(rp => rp.Role.AppId == appId))
                return Forbid("You are not authorized to delete this permission.");

            // Step 1: Delete all RolePermission links
            if (permission.RolePermissions.Any())
            {
                foreach (var rp in permission.RolePermissions.ToList())
                {
                    _rolePermissionBiz.RolePermissionRepository.Remove(rp);
                }
                await _rolePermissionBiz.SaveChangesAsync();
            }

            // Step 2: Delete the permission itself
            _permissionBiz.PermissionRepository.Remove(permission);
            await _permissionBiz.SaveChangesAsync();
            return NoContent();
        }

    }
}