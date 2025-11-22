using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Domain.Interfaces
{
    public interface IRolePermissionRepository
    {
        Task AddRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
        Task RemoveRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetRolesByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default);
        Task<int> CountByAppAsync(Guid appId, CancellationToken cancellationToken = default);
        Task ReplaceRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);
      
    }
}
