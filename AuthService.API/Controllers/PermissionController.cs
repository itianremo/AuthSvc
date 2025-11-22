using AuthService.API.Extensions;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    //[ApiExplorerSettings(GroupName = "permissions")]
    [ApiController]
    [Route("api/permissions")]
    [Authorize(Policy = "CanManagePermissions")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionBiz _permissionBiz;

        public PermissionController(IPermissionBiz permissionBiz)
        {
            _permissionBiz = permissionBiz;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var appId = this.GetAppIdFromToken();
            var isGlobalAdmin = this.IsGlobalAdminApp();

            var permissions = await _permissionBiz.GetAllPermissionsAsync(appId!.Value, isGlobalAdmin, cancellationToken);
            return Ok(permissions);
        }

        [HttpGet("{permissionId:guid}")]
        public async Task<IActionResult> GetById(Guid permissionId, CancellationToken cancellationToken)
        {
            var appId = this.GetAppIdFromToken();
            var isGlobalAdmin = this.IsGlobalAdminApp();

            try
            {
                var permission = await _permissionBiz.GetPermissionByIdAsync(permissionId, appId!.Value, isGlobalAdmin, cancellationToken);
                if (permission == null)
                    return NotFound(new { code = "PermissionNotFound", message = "Permission not found." });

                return Ok(permission);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePermissionDto dto, CancellationToken cancellationToken)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.PermissionName))
                return BadRequest(new { code = "InvalidPayload", message = "Permission name required." });

            try
            {
                var permission = await _permissionBiz.CreatePermissionAsync(dto.PermissionName.Trim(), cancellationToken);
                await _permissionBiz.AssignPermissionToRoleAsync(dto.RoleId, permission.PermissionId, cancellationToken);

                return CreatedAtAction(nameof(GetById), new { permissionId = permission.PermissionId }, new PermissionDto
                {
                    PermissionId = permission.PermissionId,
                    PermissionName = permission.PermissionName,
                    IsSystemDefined = permission.IsSystemDefined,
                    RoleId = dto.RoleId
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { code = "PermissionConflict", message = ex.Message });
            }
        }

        [HttpPut("{permissionId:guid}")]
        public async Task<IActionResult> Update(Guid permissionId, [FromBody] CreatePermissionDto dto, CancellationToken cancellationToken)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.PermissionName))
                return BadRequest(new { message = "Permission name required." });

            try
            {
                var updated = await _permissionBiz.UpdatePermissionNameAsync(permissionId, dto.PermissionName.Trim(), cancellationToken);
                return Ok(new PermissionListItemDto
                {
                    PermissionId = updated.PermissionId,
                    PermissionName = updated.PermissionName,
                    IsSystemDefined = updated.IsSystemDefined
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{permissionId:guid}")]
        public async Task<IActionResult> Delete(Guid permissionId, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _permissionBiz.DeletePermissionAsync(permissionId, cancellationToken);
                if (!success)
                    return NotFound(new { message = "Permission not found." });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{permissionId:guid}/unassign/{roleId:guid}")]
        public async Task<IActionResult> Unassign(Guid permissionId, Guid roleId, CancellationToken cancellationToken)
        {
            try
            {
                await _permissionBiz.UnassignPermissionFromRoleAsync(roleId, permissionId, cancellationToken);
                return Ok(new { message = "Permission unassigned successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
