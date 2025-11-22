using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IUserBiz : IBiz<User>
    {
        Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone, CancellationToken cancellationToken = default);
        Task<User?> GetWithRolesAndStatusesAsync(Guid userId, CancellationToken cancellationToken = default);

        // Profile management
        Task<User> UpdateEmailAsync(Guid userId, string newEmail, CancellationToken cancellationToken = default);
        Task<User> UpdatePhoneAsync(Guid userId, string newPhone, CancellationToken cancellationToken = default);
        Task<bool> ChangePasswordFromProfileAsync(Guid userId, string currentPasswordHash, string newPasswordHash, CancellationToken cancellationToken = default);

        // Role management
        Task<bool> AssignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
        Task<bool> UnassignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

        // App status management
        Task<UserAppStatus> AddUserToAppAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default);
        Task<bool> RemoveUserFromAppAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default);
        Task<UserAppStatus> UndeleteUserInAppAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default);
        Task<bool> UpdateAllAppStatusesAsync(Guid userId, string statusValue, CancellationToken cancellationToken = default);

        // Password reset
        Task<bool> AdminResetPasswordAsync(Guid adminId, Guid userId, CancellationToken cancellationToken = default);
        Task<string> InitiateForgotPasswordOtpAsync(string emailOrPhone, CancellationToken cancellationToken = default);
        Task<bool> CompleteForgotPasswordWithOtpAsync(string otpToken, string newPasswordHash, CancellationToken cancellationToken = default);

        // Verification
        Task<string> InitiateEmailVerificationAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CompleteEmailVerificationAsync(Guid userId, string otpToken, CancellationToken cancellationToken = default);
        Task<string> InitiatePhoneVerificationAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CompletePhoneVerificationAsync(Guid userId, string otpToken, CancellationToken cancellationToken = default);

        // === Auth flows ===
        Task<(bool Success, string Message, object Data)> LoginAsync(string emailOrPhone, string password, Guid appId, CancellationToken cancellationToken = default);
        Task<(bool Success, string Message, User Data)> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string Message, object Data)> RefreshTokenAsync(string refreshToken, Guid appId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UserListDto>> GetUsersPagedAsync(Guid callerAppId, bool isGlobalAdmin, int page, int pageSize, CancellationToken cancellationToken = default);

        Task<bool> SoftDeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);

    }
}
