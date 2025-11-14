using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.API.Controllers
{
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
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var user = await _userBiz.UserRepository.GetByIdAsync(userId);

            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found." });

            return Ok(new UserListDto
            {
                UserId = user.UserId,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AccountStatus = user.GlobalAccountStatus,
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>
        /// Get all users (paginated)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userBiz.UserRepository.Query()
                .Where(u => !u.IsDeleted)
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    AccountStatus = u.GlobalAccountStatus,
                    IsDeleted = u.IsDeleted,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Update user basic info
        /// </summary>
        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserBasicInfoDto dto)
        {
            if (dto is null)
                return BadRequest();

            var currentUserId = User.FindFirst("sub")?.Value;
            if (currentUserId != userId.ToString())
            {
                var hasPermission = User.FindFirst("permissions")?.Value?.Contains("ManageUsers") ?? false;
                if (!hasPermission)
                    return Forbid();
            }

            var user = await _userBiz.UserRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found." });

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailExists = await _userBiz.UserRepository.Query()
                    .AnyAsync(u => u.Email == dto.Email && u.UserId != userId);
                if (emailExists)
                    return Conflict(new { message = "Email already in use." });
                user.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            user.IsEmailVerified = dto.IsEmailVerified;
            user.IsPhoneVerified = dto.IsPhoneVerified;

            _userBiz.UserRepository.Update(user);
            await _userBiz.SaveChangesAsync();
            return Ok(new { message = "User updated successfully." });
        }

        /// <summary>
        /// Verify user email
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            if (dto is null)
                return BadRequest();

            var user = await _userBiz.UserRepository.GetByIdAsync(dto.UserId);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found." });

            // TODO: Validate OTP against stored OTP
            user.IsEmailVerified = true;
            _userBiz.UserRepository.Update(user);
            await _userBiz.SaveChangesAsync();

            return Ok(new { message = "Email verified successfully." });
        }

        /// <summary>
        /// Verify user phone
        /// </summary>
        [HttpPost("verify-phone")]
        public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneDto dto)
        {
            if (dto is null)
                return BadRequest();

            var user = await _userBiz.UserRepository.GetByIdAsync(dto.UserId);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found." });

            // TODO: Validate OTP against stored OTP
            user.IsPhoneVerified = true;
            _userBiz.UserRepository.Update(user);
            await _userBiz.SaveChangesAsync();

            return Ok(new { message = "Phone verified successfully." });
        }

        /// <summary>
        /// Soft delete user
        /// </summary>
        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var currentUserId = User.FindFirst("sub")?.Value;
            if (currentUserId != userId.ToString())
            {
                var hasPermission = User.FindFirst("permissions")?.Value?.Contains("ManageUsers") ?? false;
                if (!hasPermission)
                    return Forbid();
            }

            var user = await _userBiz.UserRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.IsDeleted = true;
            user.GlobalAccountStatus = AccountStatus.Deleted;
            _userBiz.UserRepository.Update(user);
            await _userBiz.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }
    }
}