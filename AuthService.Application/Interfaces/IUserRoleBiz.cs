using AuthService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IUserRoleBiz
    {
        // Retrieval
        Task<IEnumerable<UserRoleDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserRoleDto>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

        // Single assignment
        Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
        Task<bool> UnassignRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

        // Batch operations
        Task<bool> AssignRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);
        Task<bool> UnassignRolesFromUserAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);
    }
}
