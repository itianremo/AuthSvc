using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IUserRoleRepository
    {
        Task AddUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
        Task RemoveUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUsersByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<int> CountByAppAsync(Guid appId, CancellationToken cancellationToken = default);

    }
}
