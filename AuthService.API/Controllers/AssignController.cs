using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/assign")]
    [Authorize(Policy = "CanManageAssigns")]
    public class AssignController : ControllerBase
    {
        private readonly IUserRoleBiz _userRoleBiz;
        private readonly IRoleBiz _roleBiz;
        private readonly IPermissionBiz _permissionBiz;
        private readonly IRolePermissionBiz _rolePermissionBiz;
        private readonly IUserAppBiz _userAppBiz;
        private readonly IUserBiz _userBiz;
        private readonly IAppBiz _appBiz;

        public AssignController(
            IUserRoleBiz userRoleBiz,
            IRoleBiz roleBiz,
            IPermissionBiz permissionBiz,
            IRolePermissionBiz rolePermissionBiz,
            IUserAppBiz userAppBiz,
            IUserBiz userBiz,
            IAppBiz appBiz)
        {
            _userRoleBiz = userRoleBiz;
            _roleBiz = roleBiz;
            _permissionBiz = permissionBiz;
            _rolePermissionBiz = rolePermissionBiz;
            _userAppBiz = userAppBiz;
            _userBiz = userBiz;
            _appBiz = appBiz;
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        [HttpPost("roles")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleDto dto)
        {
            if (dto is null)
                return BadRequest();

            var user = await _userBiz.UserRepository.GetByIdAsync(dto.UserId);
            var role = await _roleBiz.RoleRepository.GetByIdAsync(dto.RoleId);
            if (user == null || role == null)
                return NotFound(new { message = "User or role not found." });

            var exists = await _userRoleBiz.UserRoleRepository.Query()
                .AnyAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);
            if (exists)
                return Conflict(new { message = "Role already assigned to user." });

            var ur = new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = dto.UserId,
                RoleId = dto.RoleId
            };
            await _userRoleBiz.UserRoleRepository.AddAsync(ur);
            await _userRoleBiz.SaveChangesAsync();

            return Ok(new { message = "Role assigned to user." });
        }

        /// <summary>
        /// Unassign role from user
        /// </summary>
        [HttpDelete("roles")]
        public async Task<IActionResult> UnassignRoleFromUser([FromBody] UnassignUserRoleDto dto)
        {
            if (dto is null)
                return BadRequest();

            var ur = await _userRoleBiz.UserRoleRepository.Query()
                .FirstOrDefaultAsync(x => x.UserId == dto.UserId && x.RoleId == dto.RoleId);
            if (ur == null)
                return NotFound(new { message = "Role assignment not found." });

            _userRoleBiz.UserRoleRepository.Remove(ur);
            await _userRoleBiz.SaveChangesAsync();

            return Ok(new { message = "Role removed from user." });
        }

        /// <summary>
        /// Assign permission to role
        /// </summary>
        [HttpPost("permissions")]
        public async Task<IActionResult> AssignPermissionToRole([FromBody] AssignPermissionDto dto)
        {
            if (dto is null)
                return BadRequest();

            var role = await _roleBiz.RoleRepository.GetByIdAsync(dto.RoleId);
            var permission = await _permissionBiz.PermissionRepository.GetByIdAsync(dto.PermissionId);
            if (role == null || permission == null)
                return NotFound(new { message = "Role or permission not found." });

            var exists = await _rolePermissionBiz.RolePermissionRepository.Query()
                .AnyAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId);
            if (exists)
                return Conflict(new { message = "Permission already assigned to role." });

            var rp = new RolePermission
            {
                RolePermissionId = Guid.NewGuid(),
                RoleId = dto.RoleId,
                PermissionId = dto.PermissionId
            };
            await _rolePermissionBiz.RolePermissionRepository.AddAsync(rp);
            await _rolePermissionBiz.SaveChangesAsync();

            return Ok(new { message = "Permission assigned to role." });
        }

        /// <summary>
        /// Unassign permission from role
        /// </summary>
        [HttpDelete("permissions")]
        public async Task<IActionResult> UnassignPermissionFromRole([FromBody] UnassignPermissionDto dto)
        {
            if (dto is null)
                return BadRequest();

            var rp = await _rolePermissionBiz.RolePermissionRepository.Query()
                .FirstOrDefaultAsync(x => x.RoleId == dto.RoleId && x.PermissionId == dto.PermissionId);
            if (rp == null)
                return NotFound(new { message = "Permission assignment not found." });

            _rolePermissionBiz.RolePermissionRepository.Remove(rp);
            await _rolePermissionBiz.SaveChangesAsync();

            return Ok(new { message = "Permission removed from role." });
        }

        /// <summary>
        /// Assign user to app
        /// </summary>
        [HttpPost("apps")]
        public async Task<IActionResult> AssignUserToApp([FromBody] UpdateUserAppStatusDto dto)
        {
            if (dto is null)
                return BadRequest();

            var app = await _appBiz.AppRepository.GetByIdAsync(dto.AppId);
            var user = await _userBiz.UserRepository.GetByIdAsync(dto.UserId);
            if (app == null || user == null)
                return NotFound(new { message = "App or user not found." });

            var exists = await _userAppBiz.UserAppRepository.Query()
                .AnyAsync(x => x.AppId == dto.AppId && x.UserId == dto.UserId);
            if (exists)
                return Conflict(new { message = "User already registered to app." });

            var ua = new UserApp
            {
                UserAppId = Guid.NewGuid(),
                AppId = dto.AppId,
                UserId = dto.UserId,
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _userAppBiz.UserAppRepository.AddAsync(ua);
            await _userAppBiz.SaveChangesAsync();

            return Ok(new { message = "User assigned to app (pending approval)." });
        }

        /// <summary>
        /// Unassign user from app
        /// </summary>
        [HttpDelete("apps")]
        public async Task<IActionResult> UnassignUserFromApp([FromBody] UpdateUserAppStatusDto dto)
        {
            if (dto is null)
                return BadRequest();

            var ua = await _userAppBiz.UserAppRepository.Query()
                .FirstOrDefaultAsync(x => x.AppId == dto.AppId && x.UserId == dto.UserId);
            if (ua == null)
                return NotFound(new { message = "User app assignment not found." });

            _userAppBiz.UserAppRepository.Remove(ua);
            await _userAppBiz.SaveChangesAsync();

            return Ok(new { message = "User unassigned from app." });
        }
    }
}