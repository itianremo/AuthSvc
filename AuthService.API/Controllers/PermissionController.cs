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

        public PermissionController(IPermissionBiz permissionBiz, IRolePermissionBiz rolePermissionBiz)
        {
            _permissionBiz = permissionBiz;
            _rolePermissionBiz = rolePermissionBiz;
        }

        /// <summary>
        /// Get all permissions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var permissions = await _permissionBiz.PermissionRepository.Query()
                .AsNoTracking()
                .Select(p => new PermissionDto
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    IsSystemDefined = p.IsSystemDefined
                })
                .ToListAsync();

            return Ok(permissions);
        }

        /// <summary>
        /// Get permission by ID
        /// </summary>
        [HttpGet("{permissionId:guid}")]
        public async Task<IActionResult> GetById(Guid permissionId)
        {
            var permission = await _permissionBiz.PermissionRepository.GetByIdAsync(permissionId);

            if (permission == null)
                return NotFound(new { message = "Permission not found." });

            return Ok(new PermissionDto
            {
                PermissionId = permission.PermissionId,
                PermissionName = permission.PermissionName,
                IsSystemDefined = permission.IsSystemDefined
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
                IsSystemDefined = false
            };

            await _permissionBiz.PermissionRepository.AddAsync(permission);
            await _permissionBiz.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { permissionId = permission.PermissionId }, new PermissionDto
            {
                PermissionId = permission.PermissionId,
                PermissionName = permission.PermissionName,
                IsSystemDefined = permission.IsSystemDefined
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

            var permission = await _permissionBiz.PermissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
                return NotFound(new { message = "Permission not found." });

            if (permission.IsSystemDefined)
                return BadRequest(new { message = "Cannot modify system-defined permission." });

            if (!string.IsNullOrWhiteSpace(dto.PermissionName))
                permission.PermissionName = dto.PermissionName;

            _permissionBiz.PermissionRepository.Update(permission);
            await _permissionBiz.SaveChangesAsync();

            return Ok(new PermissionDto
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
            var permission = await _permissionBiz.PermissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
                return NotFound(new { message = "Permission not found." });

            if (permission.IsSystemDefined)
                return BadRequest(new { message = "Cannot delete system-defined permission." });

            var hasAssignments = await _rolePermissionBiz.RolePermissionRepository.Query()
                .AnyAsync(rp => rp.PermissionId == permissionId);
            if (hasAssignments)
                return BadRequest(new { message = "Cannot delete permission with active assignments." });

            _permissionBiz.PermissionRepository.Remove(permission);
            await _permissionBiz.SaveChangesAsync();

            return NoContent();
        }
    }
}