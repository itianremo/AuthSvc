using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class RolePermissionBiz : IRolePermissionBiz
    {
        private readonly IUnitOfWork _unitOfWork;

        public RolePermissionBiz(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Retrieval
        public async Task<IEnumerable<RolePermissionDto>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var role = await _unitOfWork.Roles.GetWithPermissionsAsync(roleId, cancellationToken);
            if (role == null) return Enumerable.Empty<RolePermissionDto>();

            return role.Permissions.Select(p => new RolePermissionDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                PermissionId = p.PermissionId,
                PermissionName = p.PermissionName
            });
        }

        public async Task<IEnumerable<RolePermissionDto>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId, cancellationToken);
            if (permission == null) return Enumerable.Empty<RolePermissionDto>();

            var roles = await _unitOfWork.Roles.GetByPermissionIdAsync(permissionId, cancellationToken);
            return roles.Select(r => new RolePermissionDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                PermissionId = permission.PermissionId,
                PermissionName = permission.PermissionName
            });
        }

        // Linking
        public async Task<bool> LinkPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            var role = await _unitOfWork.Roles.GetWithPermissionsAsync(roleId, cancellationToken)
                ?? throw new InvalidOperationException("Role not found.");

            var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId, cancellationToken)
                ?? throw new InvalidOperationException("Permission not found.");

            if (role.IsSystemDefined || permission.IsSystemDefined)
                throw new InvalidOperationException("System-defined roles and permissions cannot be modified.");

            if (role.Permissions.Any(p => p.PermissionId == permissionId))
                return true;

            role.Permissions.Add(permission);
            _unitOfWork.Roles.Update(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        // Unlinking
        public async Task<bool> UnlinkPermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            var role = await _unitOfWork.Roles.GetWithPermissionsAsync(roleId, cancellationToken)
                ?? throw new InvalidOperationException("Role not found.");

            var permission = role.Permissions.FirstOrDefault(p => p.PermissionId == permissionId);
            if (permission == null) return false;

            if (permission.IsSystemDefined)
                throw new InvalidOperationException("System-defined permissions cannot be unassigned.");

            role.Permissions.Remove(permission);
            _unitOfWork.Roles.Update(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        // Batch linking
        public async Task<bool> LinkPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            foreach (var permissionId in permissionIds)
            {
                await LinkPermissionToRoleAsync(roleId, permissionId, cancellationToken);
            }
            return true;
        }

        // Batch unlinking
        public async Task<bool> UnlinkPermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            foreach (var permissionId in permissionIds)
            {
                await UnlinkPermissionFromRoleAsync(roleId, permissionId, cancellationToken);
            }
            return true;
        }


    }
}
