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
            var roles = await _roleBiz.RoleRepository.Query()
                .AsNoTracking()
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
            var roles = await _roleBiz.RoleRepository.Query()
                .Where(r => r.AppId == appId)
                .AsNoTracking()
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
        /// Get role by ID
        /// </summary>
        [HttpGet("{roleId:guid}")]
        public async Task<IActionResult> GetById(Guid roleId)
        {
            var role = await _roleBiz.RoleRepository.GetByIdAsync(roleId);

            if (role == null)
                return NotFound(new { message = "Role not found." });

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
        /// Create new role
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.RoleName))
                return BadRequest(new { message = "Role name required." });

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

            var role = await _roleBiz.RoleRepository.GetByIdAsync(roleId);
            if (role == null)
                return NotFound(new { message = "Role not found." });

            if (role.IsSystemDefined)
                return BadRequest(new { message = "Cannot modify system-defined role." });

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
            var role = await _roleBiz.RoleRepository.GetByIdAsync(roleId);
            if (role == null)
                return NotFound(new { message = "Role not found." });

            if (role.IsSystemDefined)
                return BadRequest(new { message = "Cannot delete system-defined role." });

            var hasAssignments = await _userRoleBiz.UserRoleRepository.Query()
                .AnyAsync(ur => ur.RoleId == roleId);
            if (hasAssignments)
                return BadRequest(new { message = "Cannot delete role with active assignments." });

            _roleBiz.RoleRepository.Remove(role);
            await _roleBiz.SaveChangesAsync();

            return NoContent();
        }
    }
}