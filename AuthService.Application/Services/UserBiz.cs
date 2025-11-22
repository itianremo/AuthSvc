using AuthService.Application.DTOs;
using AuthService.Application.Helpers;
using AuthService.Application.Interfaces;
using AuthService.Domain.Configs;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class UserBiz : Biz<User>, IUserBiz
    {
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _config;

        public UserBiz(IServiceProvider provider, IUnitOfWork unitOfWork, IPasswordHasher<User> hasher, IConfiguration config)
            : base(provider, unitOfWork)
        {
            _hasher = hasher;
            _config = config;
        }

        public Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone, CancellationToken cancellationToken = default)
            => UserRepository.GetByEmailOrPhoneAsync(emailOrPhone, cancellationToken);

        public Task<User?> GetWithRolesAndStatusesAsync(Guid userId, CancellationToken cancellationToken = default)
            => UserRepository.GetWithRolesAndStatusesAsync(userId, cancellationToken);

        // === Profile management ===
        public async Task<User> UpdateEmailAsync(Guid userId, string newEmail, CancellationToken cancellationToken = default)
        {
            var user = await UserRepository.GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("User not found.");
            user.Email = newEmail;
            UserRepository.Update(user);
            await SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<User> UpdatePhoneAsync(Guid userId, string newPhone, CancellationToken cancellationToken = default)
        {
            var user = await UserRepository.GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("User not found.");
            user.PhoneNumber = newPhone;
            UserRepository.Update(user);
            await SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task<bool> ChangePasswordFromProfileAsync(Guid userId, string currentPasswordHash, string newPasswordHash, CancellationToken cancellationToken = default)
        {
            var user = await UserRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;

            // Always verify the current password using the Identity hasher
            var verify = _hasher.VerifyHashedPassword(user, user.HashedPassword, currentPasswordHash);
            if (verify == PasswordVerificationResult.Failed)
                return false;

            // Set the new password using the Identity hasher
            user.HashedPassword = _hasher.HashPassword(user, newPasswordHash);

            UserRepository.Update(user);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        // === Role management ===
        public async Task<bool> AssignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.UserRoles.AddUserRoleAsync(userId, roleId, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UnassignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.UserRoles.RemoveUserRoleAsync(userId, roleId, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        // === App status management ===
        public async Task<UserAppStatus> AddUserToAppAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default)
        {
            var ua = new UserAppStatus
            {
                UserAppId = Guid.NewGuid(),
                UserId = userId,
                AppId = appId,
                Status = AppAccountStatus.Pending,
                RefreshToken = string.Empty
            };
            await _unitOfWork.UserAppStatuses.AddAsync(ua, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return ua;
        }

        public async Task<bool> RemoveUserFromAppAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default)
        {
            var ua = await _unitOfWork.UserAppStatuses.Query()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.AppId == appId, cancellationToken);
            if (ua == null) return false;

            _unitOfWork.UserAppStatuses.Remove(ua);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<UserAppStatus> UndeleteUserInAppAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default)
        {
            var ua = await _unitOfWork.UserAppStatuses.Query()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.AppId == appId && x.Status == AppAccountStatus.Deleted, cancellationToken);
            if (ua == null) throw new InvalidOperationException("User not found in app or not deleted.");

            ua.Status = AppAccountStatus.Pending;
            _unitOfWork.UserAppStatuses.Update(ua);
            await SaveChangesAsync(cancellationToken);
            return ua;
        }

        public async Task<bool> UpdateAllAppStatusesAsync(Guid userId, AppAccountStatus status, CancellationToken cancellationToken = default)
        {
            var statuses = await _unitOfWork.UserAppStatuses.GetByUserIdAsync(userId, cancellationToken);
            if (statuses == null || !statuses.Any())
                return false;

            foreach (var s in statuses)
            {
                s.Status = status;
                _unitOfWork.UserAppStatuses.Update(s);
            }

            await SaveChangesAsync(cancellationToken);
            return true;
        }

        // === Password reset ===
        public async Task<bool> AdminResetPasswordAsync(Guid adminId, Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await UserRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;
            user.HashedPassword = _hasher.HashPassword(user, Guid.NewGuid().ToString()); // random reset
            UserRepository.Update(user);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<string> InitiateForgotPasswordOtpAsync(string emailOrPhone, CancellationToken cancellationToken = default)
        {
            var user = await UserRepository.GetByEmailOrPhoneAsync(emailOrPhone, cancellationToken);
            if (user == null) return string.Empty;
            var otp = new Random().Next(100000, 999999).ToString();
            await PasswordResetTokenRepository.CreateTokenAsync(user.UserId, otp, DateTime.UtcNow.AddMinutes(10), cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return otp;
        }

        public async Task<bool> CompleteForgotPasswordWithOtpAsync(string otpToken, string newPasswordHash, CancellationToken cancellationToken = default)
        {
            var token = await PasswordResetTokenRepository.GetByTokenAsync(otpToken, cancellationToken);
            if (token == null || token.Expiry < DateTime.UtcNow) return false;
            var user = await UserRepository.GetByIdAsync(token.UserId, cancellationToken);
            if (user == null) return false;
            user.HashedPassword = newPasswordHash;
            UserRepository.Update(user);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        // === Verification ===
        public async Task<string> InitiateEmailVerificationAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var otp = Guid.NewGuid().ToString("N").Substring(0, 6);
            await PasswordResetTokenRepository.CreateTokenAsync(userId, otp, DateTime.UtcNow.AddMinutes(10), cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return otp;
        }

        public async Task<bool> CompleteEmailVerificationAsync(Guid userId, string otpToken, CancellationToken cancellationToken = default)
        {
            var token = await PasswordResetTokenRepository.GetByTokenAsync(otpToken, cancellationToken);
            if (token == null || token.Expiry < DateTime.UtcNow) return false;
            var user = await UserRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;
            user.IsEmailVerified = true;
            UserRepository.Update(user);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<string> InitiatePhoneVerificationAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            await PasswordResetTokenRepository.CreateTokenAsync(userId, otp, DateTime.UtcNow.AddMinutes(10), cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return otp;
        }

        public async Task<bool> CompletePhoneVerificationAsync(Guid userId, string otpToken, CancellationToken cancellationToken = default)
        {
            var token = await PasswordResetTokenRepository.GetByTokenAsync(otpToken, cancellationToken);
            if (token == null || token.Expiry < DateTime.UtcNow) return false;
            var user = await UserRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;
            user.IsPhoneVerified = true;
            UserRepository.Update(user);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        // === Auth flows ===
        public async Task<(bool Success, string Message, object Data)> LoginAsync(string emailOrPhone, string password, Guid appId, CancellationToken cancellationToken = default)
        {
            var user = await UserRepository.GetByEmailOrPhoneAsync(emailOrPhone, cancellationToken);
            if (user == null) return (false, "User not found.", null);

            var verify = _hasher.VerifyHashedPassword(user, user.HashedPassword, password);
            if (verify == PasswordVerificationResult.Failed)
                return (false, "Incorrect password.", null);

            var userApp = await _unitOfWork.UserAppStatuses.Query()
                .FirstOrDefaultAsync(ua => ua.UserId == user.UserId && ua.AppId == appId, cancellationToken);
            if (userApp == null)
                return (false, "User not registered for this app.", null);

            if (!StatusHelper.IsLoginAllowed(userApp.Status))
                return (false, $"Account status is {userApp.Status}.", null);

            var tokenData = await BuildJwtAndRefreshTokenAsync(user, userApp, appId, cancellationToken);
            return (true, "Login successful.", tokenData);
        }

        public async Task<(bool Success, string Message, User Data)> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default)
        {
            var app = await AppRepository.GetByIdAsync(dto.AppId, cancellationToken);
            if (app == null) return (false, "Invalid AppId.", null);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingUser = await UserRepository.Query()
                    .Include(u => u.AppStatuses)
                    .FirstOrDefaultAsync(u => u.Email == dto.Email || u.PhoneNumber == dto.PhoneNumber, cancellationToken);

                if (existingUser != null)
                {
                    var alreadyInApp = existingUser.AppStatuses?.Any(ua => ua.AppId == app.AppId) ?? false;
                    if (alreadyInApp)
                        throw new InvalidOperationException("User already registered for this app.");

                    var ua = new UserAppStatus
                    {
                        UserAppId = Guid.NewGuid(),
                        UserId = existingUser.UserId,
                        AppId = app.AppId,
                        Status = app.AutoApproveUsers ? AppAccountStatus.Approved : AppAccountStatus.Pending
                    };
                    await _unitOfWork.UserAppStatuses.AddAsync(ua, cancellationToken);
                    return (true, "Existing user attached to app.", existingUser);
                }

                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    IsEmailVerified = false,
                    IsPhoneVerified = false,
                    IsDeleted = false
                };
                user.HashedPassword = _hasher.HashPassword(user, dto.Password);

                await UserRepository.AddAsync(user, cancellationToken);

                var newUa = new UserAppStatus
                {
                    UserAppId = Guid.NewGuid(),
                    UserId = user.UserId,
                    AppId = app.AppId,
                    Status = app.AutoApproveUsers ? AppAccountStatus.Approved : AppAccountStatus.Pending
                };
                await _unitOfWork.UserAppStatuses.AddAsync(newUa, cancellationToken);

                return (true, "User registered successfully.", user);
            }, cancellationToken);
        }


        public async Task<(bool Success, string Message, object Data)> RefreshTokenAsync(string refreshToken, Guid appId, CancellationToken cancellationToken = default)
        {
            var userApp = await _unitOfWork.UserAppStatuses.GetByRefreshTokenAsync(refreshToken, cancellationToken);
            if (userApp == null || userApp.AppId != appId || userApp.RefreshTokenExpiry < DateTime.UtcNow)
                return (false, "Invalid or expired refresh token.", null);

            if (!StatusHelper.IsLoginAllowed(userApp.Status))
                return (false, $"Account status is {userApp.Status}.", null);

            var tokenData = await BuildJwtAndRefreshTokenAsync(userApp.User, userApp, appId, cancellationToken);
            return (true, "Token refreshed.", tokenData);
        }

        // === Helper: Build JWT + refresh token ===
        private async Task<object> BuildJwtAndRefreshTokenAsync(User user, UserAppStatus userApp, Guid appId, CancellationToken cancellationToken)
        {
            var roles = await _unitOfWork.UserRoles.GetRolesByUserIdAsync(user.UserId, cancellationToken);
            var roleIds = roles.Select(r => r.RoleId).ToList();

            var permissions = new List<string>();
            foreach (var roleId in roleIds)
            {
                var perms = await _unitOfWork.RolePermissions.GetPermissionsByRoleIdAsync(roleId, cancellationToken);
                permissions.AddRange(perms.Select(p => p.PermissionName));
            }
            permissions = permissions.Distinct().ToList();

            var permissionsJson = System.Text.Json.JsonSerializer.Serialize(permissions);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim("AppId", appId.ToString()),
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

            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            userApp.RefreshToken = refreshToken;
            userApp.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            userApp.LastLogin = DateTime.UtcNow;

            _unitOfWork.UserAppStatuses.Update(userApp);
            await SaveChangesAsync(cancellationToken);

            return new
            {
                token = tokenString,
                expiresAt = expires,
                refreshToken,
                refreshTokenExpiry = userApp.RefreshTokenExpiry
            };
        }


        public async Task<IReadOnlyList<UserListDto>> GetUsersPagedAsync(
            Guid callerAppId,
            bool isGlobalAdmin,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = UserRepository.Query().Where(u => !u.IsDeleted);

            if (!isGlobalAdmin)
            {
                baseQuery = baseQuery.Where(u => u.AppStatuses.Any(ua => ua.AppId == callerAppId));
            }

            var users = await baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    AccountStatus = u.AppStatuses
                        .Where(ua => ua.AppId == callerAppId)
                        .Select(ua => (AppAccountStatus?)ua.Status)   // cast to nullable
                        .FirstOrDefault().ToString() ?? AppAccountStatus.Pending.ToString(),
                    IsDeleted = u.IsDeleted,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return users;
        }


        public async Task<bool> SoftDeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await UserRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;

            user.IsDeleted = true;
            UserRepository.Update(user);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

    }
}
