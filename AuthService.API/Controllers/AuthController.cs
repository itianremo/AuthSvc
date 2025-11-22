using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.API.Controllers
{
    [ApiExplorerSettings(GroupName = "auth")]
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserBiz _userBiz;
        private readonly IUserAppStatusBiz _userAppStatusBiz;
        private readonly IPasswordResetTokenBiz _passwordResetTokenBiz;

        public AuthController(
            IUserBiz userBiz,
            IUserAppStatusBiz userAppStatusBiz,
            IPasswordResetTokenBiz passwordResetTokenBiz)
        {
            _userBiz = userBiz;
            _userAppStatusBiz = userAppStatusBiz;
            _passwordResetTokenBiz = passwordResetTokenBiz;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
        {
            var result = await _userBiz.LoginAsync(dto.EmailOrPhone, dto.Password, dto.AppId, cancellationToken);
            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(result.Data);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto, CancellationToken cancellationToken)
        {
            var result = await _userBiz.RegisterAsync(dto, cancellationToken);
            if (!result.Success)
                return Conflict(new { message = result.Message });

            return CreatedAtAction(nameof(Register), new { id = result.Data.UserId }, result.Data);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var appIdClaim = User.FindFirst("AppId")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId) || !Guid.TryParse(appIdClaim, out var appId))
                return BadRequest(new { code = "InvalidClaims", message = "Invalid token claims." });

            await _userAppStatusBiz.LogoutAsync(userId, appId, cancellationToken);
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto, CancellationToken cancellationToken)
        {
            var result = await _userBiz.RefreshTokenAsync(dto.RefreshToken, dto.AppId, cancellationToken);
            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(result.Data);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken)
        {
            var result = await _passwordResetTokenBiz.ForgotPasswordAsync(dto.Email, cancellationToken);
            return Ok(new { message = result.Message, resetToken = result.Data });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
        {
            var result = await _passwordResetTokenBiz.ResetPasswordAsync(dto.Token, dto.NewPassword, cancellationToken);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Password reset successful." });
        }

        // === NEW: Change password from profile ===
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest(new { message = "Invalid token claims." });

            var success = await _userBiz.ChangePasswordFromProfileAsync(userId, dto.CurrentPasswordHash, dto.NewPasswordHash, cancellationToken);
            if (!success)
                return BadRequest(new { message = "Password change failed." });

            return Ok(new { message = "Password changed successfully." });
        }

        // === NEW: Email verification ===
        [Authorize]
        [HttpPost("initiate-email-verification")]
        public async Task<IActionResult> InitiateEmailVerification(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest(new { message = "Invalid token claims." });

            var otp = await _userBiz.InitiateEmailVerificationAsync(userId, cancellationToken);
            return Ok(new { message = "Verification code sent.", otp });
        }

        [Authorize]
        [HttpPost("complete-email-verification")]
        public async Task<IActionResult> CompleteEmailVerification([FromBody] VerificationDto dto, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest(new { message = "Invalid token claims." });

            var success = await _userBiz.CompleteEmailVerificationAsync(userId, dto.OtpToken, cancellationToken);
            if (!success)
                return BadRequest(new { message = "Email verification failed." });

            return Ok(new { message = "Email verified successfully." });
        }

        // === NEW: Phone verification ===
        [Authorize]
        [HttpPost("initiate-phone-verification")]
        public async Task<IActionResult> InitiatePhoneVerification(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest(new { message = "Invalid token claims." });

            var otp = await _userBiz.InitiatePhoneVerificationAsync(userId, cancellationToken);
            return Ok(new { message = "Verification code sent.", otp });
        }

        [Authorize]
        [HttpPost("complete-phone-verification")]
        public async Task<IActionResult> CompletePhoneVerification([FromBody] VerificationDto dto, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest(new { message = "Invalid token claims." });

            var success = await _userBiz.CompletePhoneVerificationAsync(userId, dto.OtpToken, cancellationToken);
            if (!success)
                return BadRequest(new { message = "Phone verification failed." });

            return Ok(new { message = "Phone verified successfully." });
        }
    }
}
