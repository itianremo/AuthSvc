using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IUserAppStatusBiz : IBiz<UserAppStatus>
    {
        // Retrieval
        Task<UserAppStatus?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<UserAppStatus?> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
        Task<ICollection<UserAppStatus>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ICollection<UserAppStatus>> GetByAppIdAsync(Guid appId, CancellationToken cancellationToken = default);

        // Refresh token rotation
        Task<UserAppStatus> RotateRefreshTokenAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default);

        // Login update
        Task<bool> UpdateLastLoginAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default);

        // Status update
        Task<bool> UpdateStatusAsync(Guid userId, Guid appId, string statusValue, CancellationToken cancellationToken = default);

        Task<List<object>> GetPendingUsersAsync(Guid appId, CancellationToken cancellationToken = default);

        Task LogoutAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default);
    }
}
