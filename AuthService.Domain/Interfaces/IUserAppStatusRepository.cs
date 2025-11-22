using AuthService.Domain.Configs;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IUserAppStatusRepository : IRepository<UserAppStatus>
    {
        /// <summary>
        /// Retrieve a user-app status by refresh token.
        /// </summary>
        Task<UserAppStatus?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve all statuses for a given user.
        /// </summary>
        Task<ICollection<UserAppStatus>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve all statuses for a given app.
        /// </summary>
        Task<ICollection<UserAppStatus>> GetByAppIdAsync(Guid appId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve the first status matching a given string.
        /// </summary>
        Task<UserAppStatus?> GetByStatusAsync(AppAccountStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidate old refresh token and set a new one.
        /// </summary>
        Task InvalidateAndSetRefreshTokenAsync(Guid userId, Guid appId, string newToken, DateTime expiry, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update last login timestamp for a user in an app.
        /// </summary>
        Task UpdateLastLoginAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update status string for a user in an app.
        /// </summary>
        Task UpdateStatusAsync(Guid userId, Guid appId, AppAccountStatus statusValue, CancellationToken cancellationToken = default);
    }
}
