using AuthService.Domain.Configs;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone, CancellationToken cancellationToken = default);
        Task<int> CountByAppAsync(Guid appId, CancellationToken cancellationToken = default);
        Task<int> CountByStatusAsync(AppAccountStatus status, CancellationToken cancellationToken = default);
        Task<User?> GetWithRolesAndStatusesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<int> CountVerifiedAsync(CancellationToken cancellationToken = default);
        Task<int> CountUnverifiedAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default);

        // === Retrieve a user with roles fully loaded ===
        Task<User?> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
