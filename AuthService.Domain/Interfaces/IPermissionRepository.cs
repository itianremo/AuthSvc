using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IPermissionRepository : IRepository<Permission>
    {
        /// <summary>
        /// Retrieve a permission by its unique name.
        /// </summary>
        Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve all system-defined permissions.
        /// </summary>
        Task<IEnumerable<Permission>> GetSystemDefinedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve all permissions associated with a given role.
        /// </summary>
        Task<IEnumerable<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a permission name already exists (case-insensitive).
        /// </summary>
        Task<bool> ExistsByNameAsync(string permissionName, CancellationToken cancellationToken = default);

        Task<int> CountByAppAsync(Guid appId, CancellationToken cancellationToken = default);
    }
}
