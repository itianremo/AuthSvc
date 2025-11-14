using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserBiz _userBiz;
        private readonly IUserAppBiz _userAppBiz;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _config;

        public AuthController(IUserBiz userBiz, IUserAppBiz userAppBiz, IPasswordHasher<User> hasher, IConfiguration config)
        {
            _userBiz = userBiz;
            _userAppBiz = userAppBiz;
            _hasher = hasher;
            _config = config;
        }

        /// <summary>
        /// Login with email/phone and password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.EmailOrPhone) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Email/Phone and password required." });

            var user = await _userBiz.UserRepository.GetByEmailOrPhoneAsync(dto.EmailOrPhone);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials." });

            var verify = _hasher.VerifyHashedPassword(user, user.HashedPassword, dto.Password);
            if (verify == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid credentials." });

            if (user.IsDeleted)
                return Unauthorized(new { message = "User account deleted." });

            // Check user app status
            var userApp = await _userAppBiz.UserAppRepository.Query()
                .FirstOrDefaultAsync(ua => ua.UserId == user.UserId && ua.AppId == dto.AppId);

            if (userApp == null)
                return Unauthorized(new { message = "User not registered for this app." });

            if (userApp.AccountStatus != AccountStatus.Active && userApp.AccountStatus != AccountStatus.Approved)
                return Unauthorized(new { message = $"User account is {userApp.AccountStatus}." });

            // Build claims with permissions
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("AppId", dto.AppId.ToString()),
                new Claim("permissions", "[]")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(8);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Update last login
            userApp.LastLogin = DateTime.UtcNow;
            _userAppBiz.UserAppRepository.Update(userApp);
            await _userAppBiz.SaveChangesAsync();

            return Ok(new { token = tokenString, expiresAt = expires });
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Email and password required." });

            var exists = await _userBiz.UserRepository.Query()
                .AnyAsync(u => u.Email == dto.Email);
            if (exists)
                return Conflict(new { message = "User with this email already exists." });

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                GlobalAccountStatus = AccountStatus.Pending,
                IsEmailVerified = false,
                IsPhoneVerified = false,
                IsDeleted = false
            };
            user.HashedPassword = _hasher.HashPassword(user, dto.Password);

            await _userBiz.UserRepository.AddAsync(user);
            await _userBiz.SaveChangesAsync();

            // Register user for app
            var userApp = new UserApp
            {
                UserAppId = Guid.NewGuid(),
                UserId = user.UserId,
                AppId = dto.AppId,
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _userAppBiz.UserAppRepository.AddAsync(userApp);
            await _userAppBiz.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = user.UserId },
                new { user.UserId, user.Email });
        }

        /// <summary>
        /// Logout (token revocation managed client-side or via token blacklist)
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully." });
        }

        /// <summary>
        /// Initiate forgot password flow
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email required." });

            var user = await _userBiz.UserRepository.GetByEmailOrPhoneAsync(dto.Email);
            if (user == null)
                return Ok(new { message = "If email exists, a reset link has been sent." });

            // TODO: Generate reset token, persist with expiry, and email user
            var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            return Ok(new { message = "Reset token generated (implement email sending).", resetToken });
        }
        
    }
}