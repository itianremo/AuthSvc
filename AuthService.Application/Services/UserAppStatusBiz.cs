using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Configs;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class UserAppStatusBiz : Biz<UserAppStatus>, IUserAppStatusBiz
    {
        public UserAppStatusBiz(IServiceProvider provider, IUnitOfWork unitOfWork)
            : base(provider, unitOfWork) { }

        // === Step 1: Retrieval ===
        public Task<UserAppStatus?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => UserAppStatusRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);

        public Task<UserAppStatus?> GetByStatusAsync(AppAccountStatus status, CancellationToken cancellationToken = default)
            => UserAppStatusRepository.GetByStatusAsync(status, cancellationToken);

        public Task<ICollection<UserAppStatus>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => UserAppStatusRepository.GetByUserIdAsync(userId, cancellationToken);

        public Task<ICollection<UserAppStatus>> GetByAppIdAsync(Guid appId, CancellationToken cancellationToken = default)
            => UserAppStatusRepository.GetByAppIdAsync(appId, cancellationToken);

        // === Step 2: Refresh token rotation ===
        public async Task<UserAppStatus> RotateRefreshTokenAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default)
        {
            var newToken = Guid.NewGuid().ToString("N");
            var expiry = DateTime.UtcNow.AddDays(7);

            await UserAppStatusRepository.InvalidateAndSetRefreshTokenAsync(userId, appId, newToken, expiry, cancellationToken);
            await SaveChangesAsync(cancellationToken);

            var status = await UserAppStatusRepository.GetByRefreshTokenAsync(newToken, cancellationToken)
                ?? throw new InvalidOperationException("Failed to rotate refresh token.");

            return status;
        }

        // === Step 3: Login update ===
        public async Task<bool> UpdateLastLoginAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default)
        {
            await UserAppStatusRepository.UpdateLastLoginAsync(userId, appId, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        // === Step 4: Status update ===
        public async Task<bool> UpdateStatusAsync(Guid userId, Guid appId, AppAccountStatus statusValue, CancellationToken cancellationToken = default)
        {
            var statuses = await UserAppStatusRepository.GetByUserIdAsync(userId, cancellationToken);
            var status = statuses.FirstOrDefault(s => s.AppId == appId)
                ?? throw new InvalidOperationException("App status not found.");

            await UserAppStatusRepository.UpdateStatusAsync(userId, appId, statusValue, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<object>> GetPendingUsersAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return await UserAppStatusRepository.Query()
                .Where(ua => ua.AppId == appId && ua.Status == AppAccountStatus.Pending)
                .Include(ua => ua.User)
                .AsNoTracking()
                .Select(ua => new
                {
                    ua.User.UserId,
                    ua.User.Email,
                    ua.User.PhoneNumber,
                })
                .Cast<object>()
                .ToListAsync(cancellationToken);
        }

        // === Step 5: Logout ===
        public async Task LogoutAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default)
        {
            var status = await UserAppStatusRepository.Query()
                .FirstOrDefaultAsync(s => s.UserId == userId && s.AppId == appId, cancellationToken);

            if (status == null)
                throw new InvalidOperationException("User app status not found.");

            // Invalidate refresh token
            status.RefreshToken = string.Empty;
            status.RefreshTokenExpiry = null;

            // Mark status as pending (or suspended depending on your policy)
            status.Status = AppAccountStatus.Pending;

            UserAppStatusRepository.Update(status);
            await SaveChangesAsync(cancellationToken);
        }
    }
}
