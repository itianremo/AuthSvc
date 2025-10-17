using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Protect all endpoints
    public class AdminController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public AdminController(AuthDbContext context)
        {
            _context = context;
        }

        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole(CreateRoleDto dto)
        {
            var exists = await _context.Roles
                .AnyAsync(r => r.RoleName == dto.RoleName && r.AppId == dto.AppId);

            if (exists)
                return BadRequest("Role already exists for this app");

            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = dto.RoleName,
                AppId = dto.AppId
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(role);
        }

        [HttpPost("create-permission")]
        public async Task<IActionResult> CreatePermission(CreatePermissionDto dto)
        {
            var exists = await _context.Permissions
                .AnyAsync(p => p.PermissionName == dto.PermissionName);

            if (exists)
                return BadRequest("Permission already exists");

            var permission = new Permission
            {
                PermissionId = Guid.NewGuid(),
                PermissionName = dto.PermissionName
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return Ok(permission);
        }

        [HttpPost("assign-permission")]
        public async Task<IActionResult> AssignPermission(AssignPermissionDto dto)
        {
            var exists = await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId);

            if (exists)
                return BadRequest("Permission already assigned to role");

            var rp = new RolePermission
            {
                RolePermissionId = Guid.NewGuid(),
                RoleId = dto.RoleId,
                PermissionId = dto.PermissionId
            };

            _context.RolePermissions.Add(rp);
            await _context.SaveChangesAsync();

            return Ok("Permission assigned to role");
        }

        [HttpPost("assign-user-role")]
        public async Task<IActionResult> AssignUserRole(AssignUserRoleDto dto)
        {
            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == dto.UserId && ur.AppId == dto.AppId && ur.RoleId == dto.RoleId);

            if (exists)
                return BadRequest("User already has this role for the app");

            var userRole = new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = dto.UserId,
                AppId = dto.AppId,
                RoleId = dto.RoleId
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            return Ok("Role assigned to user for this app");
        }

        [HttpGet("user-roles")]
        public async Task<IActionResult> GetUserRoles(Guid userId, Guid appId)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.AppId == appId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();

            return Ok(roles);
        }

        [HttpDelete("unassign-permission")]
        public async Task<IActionResult> UnassignPermission([FromBody] UnassignPermissionDto dto)
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId);

            if (rolePermission == null)
                return NotFound("Permission not assigned to this role");

            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();

            return Ok("Permission unassigned from role");
        }

        [HttpDelete("unassign-user-role")]
        public async Task<IActionResult> UnassignUserRole([FromBody] UnassignUserRoleDto dto)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur =>
                    ur.UserId == dto.UserId &&
                    ur.AppId == dto.AppId &&
                    ur.RoleId == dto.RoleId);

            if (userRole == null)
                return NotFound("User does not have this role for the specified app");

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();

            return Ok("Role unassigned from user for this app");
        }

        [HttpPost("update-user-app-status")]
        public async Task<IActionResult> UpdateUserAppStatus(UpdateUserAppStatusDto dto)
        {
            var entry = await _context.UserAppStatuses
                .FirstOrDefaultAsync(x => x.UserId == dto.UserId && x.AppId == dto.AppId);

            if (entry == null)
            {
                entry = new UserAppStatus
                {
                    UserAppStatusId = Guid.NewGuid(),
                    UserId = dto.UserId,
                    AppId = dto.AppId,
                    Status = dto.Status
                };
                _context.UserAppStatuses.Add(entry);
            }
            else
            {
                entry.Status = dto.Status;
            }

            await _context.SaveChangesAsync();
            return Ok($"User status for app updated to '{dto.Status}'");
        }

    }
}
