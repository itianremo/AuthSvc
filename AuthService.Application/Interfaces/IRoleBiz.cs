using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IRoleBiz : IBiz<Role>
    {
        // Retrieval
        Task<IEnumerable<Role>> GetAllAsync(Guid? appId, bool isGlobalAdmin, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetByAppIdAsync(Guid appId, CancellationToken cancellationToken = default);
        Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<Role?> GetWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<Role?> GetWithUsersAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetSystemDefinedAsync(CancellationToken cancellationToken = default);
        Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);

        // Mutations
        Task<Role> CreateRoleAsync(Guid appId, string roleName, CancellationToken cancellationToken = default);
        Task<Role> UpdateRoleNameAsync(Guid roleId, string newName, CancellationToken cancellationToken = default);
        Task ReplaceRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);
        Task RemoveRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
        Task<bool> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    }
}
