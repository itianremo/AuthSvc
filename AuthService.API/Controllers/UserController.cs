using AuthService.Application.DTOs;
using AuthService.Application.Helpers;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _config;

        public UserController(AuthDbContext context, IPasswordHasher<User> hasher, IConfiguration config)
        {
            _context = context;
            _hasher = hasher;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email || u.PhoneNumber == dto.PhoneNumber))
                return BadRequest("User already exists");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                AppStatuses = new List<UserAppStatus>(),
                GlobalAccountStatus = "Active", // initial status
            };

            user.HashedPassword = _hasher.HashPassword(user, dto.Password);
            _context.Users.Add(user);

            var userApp = new UserApp
            {
                UserAppId = Guid.NewGuid(),
                UserId = user.UserId,
                AppId = dto.AppId,
                AccountStatus = "active",
                CreatedAt = DateTime.UtcNow
            };
            _context.UserApps.Add(userApp);

            await _context.SaveChangesAsync();
            return Ok(new { user.UserId, user.Email, user.PhoneNumber });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.EmailOrPhone || u.PhoneNumber == dto.EmailOrPhone);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var result = _hasher.VerifyHashedPassword(user, user.HashedPassword, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid credentials");

            var userApp = await _context.UserApps
                .FirstOrDefaultAsync(ua => ua.UserId == user.UserId && ua.AppId == dto.AppId);

            if (userApp == null || userApp.AccountStatus != "active") //use this to give more scenarios like suspended, deleted, etc
                return Unauthorized("Access denied for this app");

            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == user.UserId && ur.AppId == dto.AppId)
                .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .ToListAsync();

            var roles = userRoles.Select(ur => ur.Role.RoleName).ToList();
            var permissions = userRoles
                .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.PermissionName))
                .Distinct().ToList();

            var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                            new Claim("email", user.Email),
                            new Claim("phone", user.PhoneNumber),
                            new Claim("app", dto.AppId.ToString()),
                            new Claim("account_status", userApp.AccountStatus),
                            new Claim("roles", string.Join(",", roles)),
                            new Claim("permissions", string.Join(",", permissions))
                        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                        issuer: _config["Jwt:Issuer"],
                        audience: _config["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"])),
                        signingCredentials: creds
                    );

            var refreshToken = Guid.NewGuid().ToString(); // or use a secure random string
            userApp.RefreshToken = refreshToken;
            userApp.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = refreshToken,
                expiresIn = int.Parse(_config["Jwt:ExpireMinutes"]) * 60
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto dto)
        {
            var userApp = await _context.UserApps
                .Include(ua => ua.User)
                .FirstOrDefaultAsync(ua =>
                    ua.UserId == dto.UserId &&
                    ua.AppId == dto.AppId &&
                    ua.RefreshToken == dto.RefreshToken);

            if (userApp == null || userApp.AccountStatus != "active")
                return Unauthorized("Invalid or expired refresh token");

            var user = userApp.User;

            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == user.UserId && ur.AppId == dto.AppId)
                .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .ToListAsync();

            var roles = userRoles.Select(ur => ur.Role.RoleName).ToList();
            var permissions = userRoles
                .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.PermissionName))
                .Distinct().ToList();

            var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                            new Claim("email", user.Email),
                            new Claim("phone", user.PhoneNumber),
                            new Claim("app", dto.AppId.ToString()),
                            new Claim("account_status", userApp.AccountStatus),
                            new Claim("roles", string.Join(",", roles)),
                            new Claim("permissions", string.Join(",", permissions))
                        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                        issuer: _config["Jwt:Issuer"],
                        audience: _config["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"])),
                        signingCredentials: creds
                    );

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                expiresIn = int.Parse(_config["Jwt:ExpireMinutes"]) * 60
            });
        }

        [HttpPost("change-password")]
        [Authorize] // Optional: restrict to authenticated users
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound("User not found");

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.HashedPassword, dto.CurrentPassword);

            if (result == PasswordVerificationResult.Failed)
                return BadRequest("Current password is incorrect");

            user.HashedPassword = hasher.HashPassword(user, dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok("Password changed successfully");
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return NotFound("User not found");

            if (user.IsEmailVerified)
                return Ok("Email already verified");

            if (dto.Otp != "123456")
                return BadRequest("Invalid OTP");

            user.IsEmailVerified = true;
            user.GlobalAccountStatus = AccountStatusHelper.GetGlobalStatus(user);
            await _context.SaveChangesAsync();

            return Ok("Email verified");
        }

        [HttpPost("verify-phone")]
        public async Task<IActionResult> VerifyPhone(VerifyPhoneDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return NotFound("User not found");

            if (!user.IsEmailVerified)
                return BadRequest("Please verify your email first");

            if (user.IsPhoneVerified)
                return Ok("Phone already verified");

            if (dto.Otp != "123456")
                return BadRequest("Invalid OTP");

            user.IsPhoneVerified = true;
            user.GlobalAccountStatus = AccountStatusHelper.GetGlobalStatus(user);
            await _context.SaveChangesAsync();

            return Ok("Phone verified");
        }

    }

}
