using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Domain.Interfaces;
using AuthService.Domain.Configs;

namespace AuthService.Infrastructure.Repositories
{
    public class UserAppStatusRepository : Repository<UserAppStatus>, IUserAppStatusRepository
    {
        public UserAppStatusRepository(AuthDbContext context) : base(context) { }

        /// <summary>
        /// Retrieve a user-app status by refresh token.
        /// </summary>
        public async Task<UserAppStatus?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ua => ua.User)
                .Include(ua => ua.App)
                .FirstOrDefaultAsync(ua => ua.RefreshToken == refreshToken, cancellationToken);
        }

        /// <summary>
        /// Retrieve all statuses for a given user.
        /// </summary>
        public async Task<ICollection<UserAppStatus>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ua => ua.App)
                .Where(ua => ua.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieve all statuses for a given app.
        /// </summary>
        public async Task<ICollection<UserAppStatus>> GetByAppIdAsync(Guid appId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ua => ua.User)
                .Where(ua => ua.AppId == appId)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieve the first status matching a given string.
        /// </summary>
        public async Task<UserAppStatus?> GetByStatusAsync(AppAccountStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ua => ua.User)
                .Include(ua => ua.App)
                .FirstOrDefaultAsync(ua => ua.Status == status, cancellationToken);
        }

        /// <summary>
        /// Invalidate old refresh token and set a new one.
        /// </summary>
        public async Task InvalidateAndSetRefreshTokenAsync(Guid userId, Guid appId, string newToken, DateTime expiry, CancellationToken cancellationToken = default)
        {
            var status = await _dbSet.FirstOrDefaultAsync(
        s => s.UserId == userId && s.AppId == appId,
        cancellationToken);

            if (status == null) return;

            if (status.RefreshTokenExpiry.HasValue && status.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                status.RefreshToken = string.Empty;
                status.RefreshTokenExpiry = null;
            }

            status.RefreshToken = newToken;
            status.RefreshTokenExpiry = expiry;

            _dbSet.Update(status);
        }

        /// <summary>
        /// Update last login timestamp for a user in an app.
        /// </summary>
        public async Task UpdateLastLoginAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default)
        {
            var status = await _dbSet.FirstOrDefaultAsync(
        s => s.UserId == userId && s.AppId == appId,
        cancellationToken);

            if (status == null) return;

            status.LastLogin = DateTime.UtcNow;

            _dbSet.Update(status);
        }

        /// <summary>
        /// Update status string for a user in an app.
        /// </summary>
        public async Task UpdateStatusAsync(Guid userId, Guid appId, AppAccountStatus statusValue, CancellationToken cancellationToken = default)
        {
            var status = await _dbSet
                .FirstOrDefaultAsync(s => s.UserId == userId && s.AppId == appId, cancellationToken);

            if (status == null) return;

            status.Status = statusValue;

            _dbSet.Update(status);
        }
    }
}
