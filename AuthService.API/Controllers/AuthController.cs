using AuthService.Application.DTOs;
using AuthService.Application.Helpers;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
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
        private readonly IAppBiz _appBiz;
        private readonly IUserRoleBiz _userRoleBiz;
        private readonly IUserAppStatusBiz _userAppStatusBiz;
        private readonly IPasswordResetBiz _passwordResetBiz;

        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _config;

        public AuthController(IUserBiz userBiz,
            IUserAppBiz userAppBiz,
            IAppBiz appBiz,
            IUserRoleBiz userRoleBiz,
            IUserAppStatusBiz userAppStatusBiz,
            IPasswordHasher<User> hasher,
            IConfiguration config)
        {
            _userBiz = userBiz;
            _userAppBiz = userAppBiz;
            _appBiz = appBiz;
            _userRoleBiz = userRoleBiz;
            _userAppStatusBiz = userAppStatusBiz;
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

            // Lookup user by email or phone
            var user = await _userBiz.UserRepository.GetByEmailOrPhoneAsync(dto.EmailOrPhone);
            if (user == null)
            {
                // Indicate whether the lookup was by email or phone
                var isEmail = dto.EmailOrPhone.Contains('@');
                return Unauthorized(new { message = isEmail ? 
                    "No user found with this email." 
                    : "No user found with this phone number." 
                });
            }

            // Verify password
            var verify = _hasher.VerifyHashedPassword(user, user.HashedPassword, dto.Password);
            if (verify == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Incorrect password." });

            // Deleted accounts: behave same as register case
            if (user.IsDeleted)
            {
                user.IsDeleted = false; // For now, auto-recover until recovery flow is implemented
                user.IsPhoneVerified = false;
                user.IsEmailVerified = false;

                _userBiz.UserRepository.Update(user);
                await _userBiz.SaveChangesAsync();

                return Unauthorized(new { message = "Account is deleted. Please recover your account via email before re-login again. Now it is recovered until implementing this function." });
            }
            // Ensure the user is registered for this app
            var userApp = await _userAppBiz.UserAppRepository.Query()
                .FirstOrDefaultAsync(ua => ua.UserId == user.UserId && ua.AppId == dto.AppId);

            if (userApp == null)
                return Conflict(new { message = "User not registered for this app. Please register first." });

            // Normalize status checks to string to support either enum or string-backed properties
            var globalStatus = user.GlobalAccountStatus?.ToString() ?? string.Empty;
            var appStatus = userApp.AccountStatus?.ToString() ?? string.Empty;

            // Check global account status
            if (!StatusHelper.IsLoginAllowed(user.GlobalAccountStatus))
                return Unauthorized(new { message = $"Global account status is {user.GlobalAccountStatus}." });

            // Check app-specific account status
            if (!StatusHelper.IsLoginAllowed(userApp.AccountStatus))
                return Unauthorized(new { message = $"Account status for this app is {userApp.AccountStatus}." });

            // All checks passed — create JWT
            // Fetch roles for this user in this app
            var roleIds = await _userRoleBiz.UserRoleRepository.Query()
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            // Fetch permissions for those roles
            var permissions = await _userRoleBiz.RolePermissionRepository.Query()
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission.PermissionName)
                .Distinct()
                .ToListAsync();

            var permissionsJson = System.Text.Json.JsonSerializer.Serialize(permissions);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("AppId", dto.AppId.ToString()),
                new Claim("permissions", permissionsJson)
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

            // Generate and persist refresh token
            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            userApp.RefreshToken = refreshToken;
            userApp.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            userApp.LastLogin = DateTime.UtcNow;

            _userAppBiz.UserAppRepository.Update(userApp);
            await _userAppBiz.SaveChangesAsync();

            return Ok(new
            {
                token = tokenString,
                expiresAt = expires,
                refreshToken,
                refreshTokenExpiry = userApp.RefreshTokenExpiry
            });
        }

        /// <summary>
        /// Register a new user
        /// Scenarios implemented:
        /// - Existing user:
        ///   * Deleted account -> refuse and instruct to recover via email
        ///   * Already registered with this app -> refuse
        ///   * Registered with other app(s) -> attach user to this app, assign default role, update email/phone and verification flags
        /// - New user:
        ///   * Create user, create app registration (UserApp), create/assign default role
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (dto is null
                || string.IsNullOrWhiteSpace(dto.Email)
                || string.IsNullOrWhiteSpace(dto.PhoneNumber)
                || string.IsNullOrWhiteSpace(dto.Password)
                || dto.AppId == Guid.Empty)
                return BadRequest(new { message = "Email, phone, AppId and password are required." });

            var app = await _appBiz.AppRepository.GetByIdAsync(dto.AppId);
            if (app == null)
                return BadRequest(new { message = "Invalid AppId." });

            // Try find an existing user by email or phone and include related app/roles
            var existingUser = await _userBiz.UserRepository.Query()
                .Include(u => u.UserApps)
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Email == dto.Email || u.PhoneNumber == dto.PhoneNumber);

            if (existingUser != null)
            {
                // Deleted account: refuse, instruct to recover via email
                if (existingUser.IsDeleted)
                {
                    existingUser.IsDeleted = false; // For now, auto-recover until recovery flow is implemented
                    existingUser.IsPhoneVerified = false;
                    existingUser.IsEmailVerified = false;

                    _userBiz.UserRepository.Update(existingUser);
                    await _userBiz.SaveChangesAsync();

                    return Conflict(new { message = "Account is deleted. Please recover your account via email before re-registering. Now it is recovered until implementing this function." });
                }

                // Already registered with this app: refuse
                var alreadyInApp = existingUser.UserApps?.Any(ua => ua.AppId == app.AppId) ?? false;
                if (alreadyInApp)
                {
                    return Conflict(new { message = "User with this email/phone is already registered for this app." });
                }

                // Registered with other app(s): attach to this app, assign default role, update verification status
                var userApp = new UserApp
                {
                    UserAppId = Guid.NewGuid(),
                    UserId = existingUser.UserId,
                    AppId = app.AppId,
                    AccountStatus = app.AutoApproveUsers ? AccountStatus.Approved : AccountStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await _userAppBiz.UserAppRepository.AddAsync(userApp);
                await _userAppBiz.SaveChangesAsync();

                // Update email/phone and verification flags
                existingUser.IsPhoneVerified = false;
                existingUser.IsEmailVerified = false;
                _userBiz.UserRepository.Update(existingUser);
                await _userBiz.SaveChangesAsync();

                // Ensure app status is recorded (if using UserAppStatus)
                await _userAppStatusBiz.UserAppStatusRepository.AddAsync(new UserAppStatus
                {
                    UserAppStatusId = Guid.NewGuid(),
                    UserId = existingUser.UserId,
                    AppId = app.AppId,
                    Status = app.AutoApproveUsers ? AccountStatus.Approved : AccountStatus.Pending,
                    //UpdatedAt = DateTime.UtcNow
                });
                await _userAppStatusBiz.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(Register),
                    new { id = existingUser.UserId },
                    new { existingUser.UserId, existingUser.Email, message = "Existing user attached to app." });
            }

            // New user flow
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                GlobalAccountStatus = app.AutoApproveUsers ? AccountStatus.Approved : AccountStatus.Pending,
                IsEmailVerified = false,
                IsPhoneVerified = false,
                IsDeleted = false
            };
            user.HashedPassword = _hasher.HashPassword(user, dto.Password);

            await _userBiz.UserRepository.AddAsync(user);
            await _userBiz.SaveChangesAsync();

            // Register user for app
            var newUserApp = new UserApp
            {
                UserAppId = Guid.NewGuid(),
                UserId = user.UserId,
                AppId = app.AppId,
                AccountStatus = app.AutoApproveUsers ? AccountStatus.Approved : AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _userAppBiz.UserAppRepository.AddAsync(newUserApp);
            await _userAppBiz.SaveChangesAsync();

            // Ensure app status is recorded (if using UserAppStatus)
            await _userAppStatusBiz.UserAppStatusRepository.AddAsync(new UserAppStatus
            {
                UserAppStatusId = Guid.NewGuid(),
                UserId = user.UserId,
                AppId = app.AppId,
                Status = app.AutoApproveUsers ? AccountStatus.Approved : AccountStatus.Pending,
                //UpdatedAt = DateTime.UtcNow
            });
            await _userAppStatusBiz.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Register),
                new { id = user.UserId },
                new { user.UserId, user.Email });
        }

        /// <summary>
        /// Logout (token revocation managed client-side or via token blacklist)
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Extract user id and app id from the JWT
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest(new { message = "Invalid user in token." });

            var appIdClaim = User.FindFirst("AppId")?.Value;
            if (!Guid.TryParse(appIdClaim, out var appId))
                return BadRequest(new { message = "AppId missing in token." });

            // Find the UserApp record for this user+app
            var userApp = await _userAppBiz.UserAppRepository.Query()
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AppId == appId);

            if (userApp == null)
                return Ok(new { message = "No active session found for this user and app." });

            // Remove the stored refresh token and persist
            if (!string.IsNullOrEmpty(userApp.RefreshToken))
            {
                userApp.RefreshToken = string.Empty;
                userApp.RefreshTokenExpiry = null;
                _userAppBiz.UserAppRepository.Update(userApp);
                await _userAppBiz.SaveChangesAsync();
            }

            return Ok(new { message = "Logged out successfully." });

            //alternate way
            //var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            //var appId = Guid.Parse(User.FindFirstValue("AppId"));

            //var userApp = await _userAppBiz.UserAppRepository.Query()
            //    .FirstOrDefaultAsync(ua => ua.UserId == Guid.Parse(userId) && ua.AppId == appId);

            //if (userApp != null)
            //{
            //    userApp.RefreshToken = string.Empty;
            //    userApp.RefreshTokenExpiry = null;
            //    _userAppBiz.UserAppRepository.Update(userApp);
            //    await _userAppBiz.SaveChangesAsync();
            //}

            //return Ok(new { message = "Logged out successfully." });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.RefreshToken) || dto.AppId == Guid.Empty)
                return BadRequest(new { message = "Refresh token and AppId required." });

            var userApp = await _userAppBiz.UserAppRepository.Query()
                .Include(ua => ua.User)
                .FirstOrDefaultAsync(ua => ua.RefreshToken == dto.RefreshToken && ua.AppId == dto.AppId);

            if (userApp == null || userApp.RefreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            var user = userApp.User;

            // Normalize status checks to string to support either enum or string-backed properties
            var globalStatus = user.GlobalAccountStatus?.ToString() ?? string.Empty;
            var appStatus = userApp.AccountStatus?.ToString() ?? string.Empty;

            // Check global account status
            if (!StatusHelper.IsLoginAllowed(user.GlobalAccountStatus))
                return Unauthorized(new { message = $"Global account status is {user.GlobalAccountStatus}." });

            // Check app-specific account status
            if (!StatusHelper.IsLoginAllowed(userApp.AccountStatus))
                return Unauthorized(new { message = $"Account status for this app is {userApp.AccountStatus}." });

            // All checks passed — create JWT
            // Fetch roles for this user in this app
            var roleIds = await _userRoleBiz.UserRoleRepository.Query()
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            // Fetch permissions for those roles
            var permissions = await _userRoleBiz.RolePermissionRepository.Query()
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission.PermissionName)
                .Distinct()
                .ToListAsync();

            var permissionsJson = System.Text.Json.JsonSerializer.Serialize(permissions);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("AppId", dto.AppId.ToString()),
                new Claim("permissions", permissionsJson)
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

            // Generate and persist refresh token
            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            userApp.RefreshToken = refreshToken;
            userApp.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            userApp.LastLogin = DateTime.UtcNow;

            _userAppBiz.UserAppRepository.Update(userApp);
            await _userAppBiz.SaveChangesAsync();

            return Ok(new
            {
                token = tokenString,
                expiresAt = expires,
                refreshToken,
                refreshTokenExpiry = userApp.RefreshTokenExpiry
            });
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

            var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            await _passwordResetBiz.PasswordResetTokenRepository.AddAsync(new PasswordResetToken
            {
                UserId = user.UserId,
                Token = resetToken,
                Expiry = DateTime.UtcNow.AddHours(1)
            });
            await _passwordResetBiz.SaveChangesAsync();

            // TODO: Send email with token
            return Ok(new { message = "Reset token generated and stored. Email sending pending.", resetToken });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { message = "Token and new password required." });

            var tokenEntry = await _passwordResetBiz.PasswordResetTokenRepository.Query()
                .FirstOrDefaultAsync(t => t.Token == dto.Token);

            if (tokenEntry == null || tokenEntry.Expiry < DateTime.UtcNow)
                return BadRequest(new { message = "Invalid or expired token." });

            var user = await _userBiz.UserRepository.GetByIdAsync(tokenEntry.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.HashedPassword = _hasher.HashPassword(user, dto.NewPassword);
            _userBiz.UserRepository.Update(user);
            await _userBiz.SaveChangesAsync();

            return Ok(new { message = "Password reset successful." });
        }

    }
}