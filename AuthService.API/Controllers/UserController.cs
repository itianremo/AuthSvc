using AuthService.API.Extensions;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Configs;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    //[ApiExplorerSettings(GroupName = "users")]
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserBiz _userBiz;

        public UserController(IUserBiz userBiz)
        {
            _userBiz = userBiz;
        }

        /// <summary>
        /// Get user profile by ID
        /// </summary>
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserById(Guid userId, CancellationToken cancellationToken)
        {
            var callerAppId = this.GetAppIdFromToken();

            var user = await _userBiz.GetWithRolesAndStatusesAsync(userId, cancellationToken);
            if (user == null || user.IsDeleted)
                //todo:make error codes consistent like this
                return NotFound(new { code = "UserNotFound", message = "User not found." });

            // Non-global users can only access users from their own app
            if (!this.IsGlobalAdminApp() && !user.AppStatuses.Any(ua => ua.AppId == callerAppId))
                return Unauthorized(new
                {
                    code = "UnauthorizedAccess",
                    message = "You are not authorized to access this user."
                });

            return Ok(new UserListDto
            {
                UserId = user.UserId,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AccountStatus = user.AppStatuses.FirstOrDefault(ua => ua.AppId == callerAppId)?.Status.ToString() ?? AppAccountStatus.Pending.ToString(),
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>
        /// Get all users (paginated)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var callerAppIdNullable = this.GetAppIdFromToken();
            if (!callerAppIdNullable.HasValue)
                return BadRequest(new { code = "MissingClaim", message = "AppId claim missing." });

            var callerAppId = callerAppIdNullable.Value;
            var isGlobalAdmin = this.IsGlobalAdminApp();

            var users = await _userBiz.GetUsersPagedAsync(callerAppId, isGlobalAdmin, page, pageSize, cancellationToken);
            return Ok(users);
        }

        /// <summary>
        /// Update user basic info
        /// </summary>
        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserBasicInfoDto dto, CancellationToken cancellationToken)
        {
            if (dto is null)
                return BadRequest();

            var user = await _userBiz.GetWithRolesAndStatusesAsync(userId, cancellationToken);
            if (user == null || user.IsDeleted)
                return BadRequest(new { code = "InvalidPayload", message = "Request body is required." });

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                await _userBiz.UpdateEmailAsync(userId, dto.Email, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                await _userBiz.UpdatePhoneAsync(userId, dto.PhoneNumber, cancellationToken);
            }

            //user.IsEmailVerified = dto.IsEmailVerified;
            //user.IsPhoneVerified = dto.IsPhoneVerified;

            await _userBiz.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "User updated successfully." });
        }

        /// <summary>
        /// Verify user email
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto, CancellationToken cancellationToken)
        {
            var success = await _userBiz.CompleteEmailVerificationAsync(dto.UserId, dto.OtpToken, cancellationToken);
            if (!success)
                return BadRequest(new { message = "Email verification failed." });

            return Ok(new { message = "Email verified successfully." });
        }

        /// <summary>
        /// Verify user phone
        /// </summary>
        [HttpPost("verify-phone")]
        public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneDto dto, CancellationToken cancellationToken)
        {
            var success = await _userBiz.CompletePhoneVerificationAsync(dto.UserId, dto.OtpToken, cancellationToken);
            if (!success)
                return BadRequest(new { message = "Phone verification failed." });

            return Ok(new { message = "Phone verified successfully." });
        }

        /// <summary>
        /// Soft delete user
        /// </summary>
        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken cancellationToken)
        {
            var success = await _userBiz.SoftDeleteUserAsync(userId, cancellationToken);
            if (!success)
                return NotFound(new { message = "User not found." });

            return Ok(new { message = "User deleted successfully." });
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        [HttpPost("{userId:guid}/assign-role/{roleId:guid}")]
        public async Task<IActionResult> AssignRole(Guid userId, Guid roleId, CancellationToken cancellationToken)
        {
            var success = await _userBiz.AssignRoleAsync(userId, roleId, cancellationToken);
            if (!success)
                return BadRequest(new { message = "Role assignment failed." });

            return Ok(new { message = "Role assigned successfully." });
        }

        /// <summary>
        /// Unassign role from user
        /// </summary>
        [HttpPost("{userId:guid}/unassign-role/{roleId:guid}")]
        public async Task<IActionResult> UnassignRole(Guid userId, Guid roleId, CancellationToken cancellationToken)
        {
            var success = await _userBiz.UnassignRoleAsync(userId, roleId, cancellationToken);
            if (!success)
                return BadRequest(new { message = "Role unassignment failed." });

            return Ok(new { message = "Role unassigned successfully." });
        }

        /// <summary>
        /// Update all app statuses for a user
        /// </summary>
        [HttpPost("{userId:guid}/update-status")]
        public async Task<IActionResult> UpdateStatus(Guid userId, [FromBody] UpdateStatusDto dto, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<AppAccountStatus>(dto.StatusValue, true, out var statusEnum))
                return BadRequest(new { code = "InvalidStatus", message = "Invalid status value." });

            var success = await _userBiz.UpdateAllAppStatusesAsync(userId, statusEnum, cancellationToken);
            
            if (!success)
                return BadRequest(new { message = "Status update failed." });

            return Ok(new { message = "Status updated successfully." });
        }

        /// <summary>
        /// Undelete user in app
        /// </summary>
        [HttpPost("{userId:guid}/undelete/{appId:guid}")]
        public async Task<IActionResult> UndeleteUser(Guid userId, Guid appId, CancellationToken cancellationToken)
        {
            var ua = await _userBiz.UndeleteUserInAppAsync(userId, appId, cancellationToken);
            return Ok(new { message = "User undeleted successfully.", ua });
        }
    }
}
