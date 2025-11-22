using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IPermissionBiz : IBiz<Permission>
    {
        Task<Permission?> GetByNameAsync(string permissionName, CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> GetSystemDefinedAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

        Task<Permission> CreatePermissionAsync(string permissionName, CancellationToken cancellationToken = default);
        Task<Permission> UpdatePermissionNameAsync(Guid permissionId, string newName, CancellationToken cancellationToken = default);
        Task<bool> DeletePermissionAsync(Guid permissionId, CancellationToken cancellationToken = default);

        Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
        Task UnassignPermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);

        Task<IEnumerable<PermissionListItemDto>> GetAllPermissionsAsync(Guid appId, bool isGlobalAdmin, CancellationToken cancellationToken = default);
        Task<PermissionDto?> GetPermissionByIdAsync(Guid permissionId, Guid appId, bool isGlobalAdmin, CancellationToken cancellationToken = default);
    }
}
