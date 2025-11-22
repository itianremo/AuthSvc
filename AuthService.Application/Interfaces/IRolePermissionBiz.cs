using AuthService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IRolePermissionBiz
    {
        // Retrieval
        Task<IEnumerable<RolePermissionDto>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RolePermissionDto>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default);

        // Linking/unlinking
        Task<bool> LinkPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
        Task<bool> UnlinkPermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);

        // Batch operations
        Task<bool> LinkPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);
        Task<bool> UnlinkPermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);
    }
}
