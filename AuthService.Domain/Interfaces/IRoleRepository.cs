using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<IEnumerable<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetByAppIdAsync(Guid appId, CancellationToken cancellationToken = default);
        Task<Role?> GetWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetSystemDefinedAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(Guid appId, string roleName, CancellationToken cancellationToken = default);
        Task<bool> IsNameSimilarToSystemRoleAsync(string roleName, CancellationToken cancellationToken = default);
        Task ReplaceRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);
        Task AddUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
        Task RemoveRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);

        // ===  Retrieve all roles that include a given permission ===
        Task<IEnumerable<Role>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default);
        Task<Role?> GetWithUsersAsync(Guid roleId, CancellationToken cancellationToken = default);

        Task<int> CountByAppAsync(Guid appId, CancellationToken cancellationToken = default);
    }
}
